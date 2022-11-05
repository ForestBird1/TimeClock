using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;

//Json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//GoogleAPIs
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace TimeClock
{
    public enum WorkStatus
    {
        일하는중,
        일하는중_쉬는시간없음,
        쉬는중,
        퇴근
    }

    public partial class TimeClock : Form
    {
        private const int __MAX_BREAK_TIME = 5;
        public struct TimeClockData
        {

            public int access_row;
            public string access_date;
            public string time_work_total;
            public string time_work_start;
            public string time_work_end;

            public WorkStatus work_status;

            public string[] break_times;

            public int break_time_count;

            //프로그램이 실행될 때 딱 한번만 호출됩니다
            public void PostInitTimeClockData()
            {
                access_row = -1;
                access_date = "";
                time_work_total = "";
                time_work_start = "";
                time_work_end = "";

                work_status = WorkStatus.퇴근;

                break_times = new string[__MAX_BREAK_TIME];
                for (int i = 0; i < __MAX_BREAK_TIME; ++i)
                {
                    break_times[i] = "";
                }

                break_time_count = 0;
            }

            //퇴근할 때마다 초기화됩니다
            public void InitWorkEndTimeClockData()
            {
                access_date = "";
                time_work_total = "";
                time_work_start = "";
                time_work_end = "";

                for (int i = 0; i < __MAX_BREAK_TIME; ++i)
                {
                    break_times[i] = "";
                }

                break_time_count = 0;
            }
        };

        //데이터파일 경로들
        private readonly string _data_time_clock = Environment.CurrentDirectory + "\\Data\\" + "data_time_clock.json";
        private readonly string _data_sheet = Environment.CurrentDirectory + "\\Data\\Client\\" + "data_sheet.txt";
        private readonly string _data_client = Environment.CurrentDirectory + "\\Data\\Client\\client_secret.json";
        private readonly string _data_token_folder = "Data\\Client";

        private string _sheet_id = "";
        private string _sheet_name = "";
        private IList<IList<Object>> _select_data = null;
        private TimeClockData _time_clock_data;

        //구글api인증여부
        private bool _is_credential = false;        

        //쉬는시간 문장입니다
        private readonly string _break_time_start = "쉬는시간_시작";
        private readonly string _break_time_end = "쉬는시간_종료";

        //api로 데이터를 보낼때 사용됩니다. 사용하기전 Clear()를 꼭 호출하고 사용하세요.
        private List<object> _sheet_data = new List<object>();

        public TimeClock()
        {
            InitializeComponent();

            PostInitApp();
        }

        
        private void PostInitApp()
        {
            _time_clock_data.PostInitTimeClockData();
            lb_message.Text = "실행중...";

            //구글시트정보를 가져옵니다
            string[] arr_data_sheet = File.ReadAllLines(_data_sheet);
            _sheet_id = arr_data_sheet[0];
            _sheet_name = arr_data_sheet[1];


            //미리 가져온 데이터파일이 있는지 확인합니다
            if (File.Exists(_data_time_clock))
            {
                //데이터파일이 있습니다. 데이터를 불러옵니다
                using (StreamReader sr_data =  File.OpenText(_data_time_clock))
                    using (JsonTextReader json_reader = new JsonTextReader(sr_data))
                {
                    //텍스트형식의 json데이터를 오브젝트형태로 변경합니다
                    JObject json_obj = (JObject)JToken.ReadFrom(json_reader);

                    _time_clock_data.access_row = int.Parse(json_obj["access_row"].ToString());
                    _time_clock_data.access_date = json_obj["access_date"].ToString();
                    _time_clock_data.time_work_total = json_obj["time_work_total"].ToString();
                    _time_clock_data.time_work_start = json_obj["time_work_start"].ToString();
                    _time_clock_data.time_work_end = json_obj["time_work_end"].ToString();

                    SetWorkStatus((WorkStatus)Enum.Parse(typeof(WorkStatus), json_obj["work_status"].ToString()), false);

                    for(int i = 1; i<=__MAX_BREAK_TIME; ++i)
                    {
                        _time_clock_data.break_times[i - 1] = json_obj["time_break_time_" + i.ToString()].ToString();
                    }

                    _time_clock_data.break_time_count = int.Parse(json_obj["break_time_count"].ToString());
                }
            }
            else
            {
                //데이터파일이 없습니다 구글api를 사용하기위해 인증합니다
                DoCredential();

                //엑세스해야할 행을 구합니다
                SelectData("A1", out _select_data);
                _time_clock_data.access_row = int.Parse(_select_data[0][0].ToString());

                //엑세스해야할 행에 이미 데이터가 있는지 확인합니다
                SelectData("B" + _time_clock_data.access_row.ToString() + ":" + "K" + _time_clock_data.access_row.ToString(), out _select_data);
                if (_select_data != null && _select_data.Count > 0)
                {
                    //데이터가 있습니다.
                    //각 열에 맞는 값을 구조체에 입력합니다
                    for (int i = 0; i < _select_data[0].Count(); ++i)
                    {
                        switch(i)
                        {
                            case 0:
                                //날짜
                                _time_clock_data.access_date = _select_data[0][i].ToString();
                                break;
                            case 1:
                                //근무시간
                                _time_clock_data.time_work_total = _select_data[0][i].ToString();
                                break;
                            case 2:
                                //출근시간
                                _time_clock_data.time_work_start = _select_data[0][i].ToString();
                                break;
                            case 3:
                                //퇴근시간
                                _time_clock_data.time_work_end = _select_data[0][i].ToString();
                                break;
                            case 4:
                                //쉬는중?
                                SetWorkStatus((WorkStatus)Enum.Parse(typeof(WorkStatus), _select_data[0][i].ToString()), false);
                                break;

                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                //쉬는시간1
                                _time_clock_data.break_times[i - __MAX_BREAK_TIME] = _select_data[0][i].ToString();
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    //아무것도 없으므로 어제 잘 퇴근한 상태입니다
                    SetWorkStatus(WorkStatus.퇴근, false);
                }

                //저장
                JsonSave();
            }
        }


        private void btn_work_start_Click(object sender, EventArgs e)
        {
            //출근합니다
            //이때 이미 엑세스할 row값을 가지고 있어야합니다!!
            //출근을 기록하기 시작한 날짜, 출근날짜를 기록합니다. 일상태도 변경합니다
            _time_clock_data.access_date = GetDateTime(false); // B
            _time_clock_data.time_work_start = GetDateTime(true); // D
            SetWorkStatus(WorkStatus.일하는중, false); // F

            //구글 시트에 추가합니다
            _sheet_data.Clear();
            _sheet_data.Add(_time_clock_data.access_date);
            _sheet_data.Add("");
            _sheet_data.Add(_time_clock_data.time_work_start);
            _sheet_data.Add("");
            _sheet_data.Add(_time_clock_data.work_status.ToString());
            InsertData("B" + _time_clock_data.access_row.ToString() + ":F" + _time_clock_data.access_row.ToString(), ref _sheet_data);

            //Json으로 저장합니다
            JsonSave();
        }
        private void btn_break_toggle_Click(object sender, EventArgs e)
        {
            //쉬는시간을 시작 및 종료하는 토글버튼입니다
            if(_time_clock_data.work_status == WorkStatus.쉬는중)
            {
                //쉬는중상태에서 눌렀습니다. 쉬는것을 종료합니다
                if (_time_clock_data.break_time_count + 1 >= __MAX_BREAK_TIME)
                    SetWorkStatus(WorkStatus.일하는중_쉬는시간없음, true);
                else
                    SetWorkStatus(WorkStatus.일하는중, true);

                _time_clock_data.break_times[_time_clock_data.break_time_count] += " ~ " + "\r\n" + GetDateTime(true);

                //얼마나 쉬었는지 계산합니다
                DateTime date_prev;
                DateTime date_now;
                string[] arr_break_time_interval = _time_clock_data.break_times[_time_clock_data.break_time_count].Split('~');
                date_prev = DateTime.Parse(arr_break_time_interval[0]);
                date_now = DateTime.Parse(arr_break_time_interval[1]);
                _time_clock_data.break_times[_time_clock_data.break_time_count] += " = " + new TimeSpan((date_now - date_prev).Ticks).ToString();

                //구글 시트에 추가합니다 //G,H,I,J,K(71~75)
                char chr_column = (char)(_time_clock_data.break_time_count + 71);
                _sheet_data.Clear();
                _sheet_data.Add(_time_clock_data.break_times[_time_clock_data.break_time_count]);
                InsertData(chr_column + _time_clock_data.access_row.ToString(), ref _sheet_data);

                //값을 변경합니다
                _time_clock_data.break_time_count += 1;
            }
            else
            {
                //쉬는상태로 변경합니다
                SetWorkStatus(WorkStatus.쉬는중, true);
                _time_clock_data.break_times[_time_clock_data.break_time_count] = GetDateTime(true);

                //구글 시트에 추가합니다 //G,H,I,J,K(71~75)
                char chr_column = (char)(_time_clock_data.break_time_count + 71);
                _sheet_data.Clear();
                _sheet_data.Add(_time_clock_data.break_times[_time_clock_data.break_time_count]);
                InsertData(chr_column + _time_clock_data.access_row.ToString(), ref _sheet_data);
            }

            //Json으로 저장합니다
            JsonSave();
        }

        private void btn_work_end_Click(object sender, EventArgs e)
        {
            //퇴근합니다. 근무상태api는 밑에서 같이 올립니다
            SetWorkStatus(WorkStatus.퇴근, false);
            _time_clock_data.time_work_end = GetDateTime(true);

            //얼마나 근무했는지 계산합니다
            DateTime date_prev;
            DateTime date_now;
            date_prev = DateTime.Parse(_time_clock_data.time_work_start);
            date_now = DateTime.Parse(_time_clock_data.time_work_end);
            long l_total_tick = (date_now - date_prev).Ticks;
            //_time_clock_data.time_work_total = new TimeSpan((date_now - date_prev).Ticks).ToString();

            //휴게시간까지 계산합니다
            long l_break_time_total_tick = 0;
            for(int i = 0; i<_time_clock_data.break_time_count; ++i)
            {
                l_break_time_total_tick += TimeSpan.Parse(_time_clock_data.break_times[i].Split('=')[1]).Ticks;
            }
            _time_clock_data.time_work_total = new TimeSpan(l_total_tick - l_break_time_total_tick).ToString();


            //구글 시트에 올립니다
            //엑세스해야할 행값을 추가하고 올립니다
            _time_clock_data.access_row += 1;
            _sheet_data.Clear();
            _sheet_data.Add(_time_clock_data.access_row);
            InsertData("A1", ref _sheet_data);

            //총 근무시간을 올립니다
            _sheet_data.Clear();
            _sheet_data.Add(_time_clock_data.time_work_total);
            InsertData("C" + (_time_clock_data.access_row - 1).ToString(), ref _sheet_data);

            //퇴근시간과 근무상태를 올립니다
            _sheet_data.Clear();
            _sheet_data.Add(_time_clock_data.time_work_end);
            _sheet_data.Add(_time_clock_data.work_status.ToString());
            InsertData("E" + (_time_clock_data.access_row - 1).ToString(), ref _sheet_data);

            //퇴근했으므로 데이터를 초기화해줍니다
            _time_clock_data.InitWorkEndTimeClockData();

            //Json으로 저장합니다
            JsonSave();
        }

        private string GetDateTime(bool b_is_all)
        {
            if(b_is_all)
            {
                //return DateTime.Now.ToString("yyyy년-MM월-dd일-HH시-mm분-ss초");
                return DateTime.Now.ToString();
            }
            else
            {
                return DateTime.Now.ToString("yyyy년-MM월-dd일");
            }
        }

        private void ChangeButtonStatusByWorkStatus()
        {
            switch (_time_clock_data.work_status)
            {
                case WorkStatus.일하는중:
                    //퇴근버튼과 쉬는시간버튼 활성화
                    btn_work_start.Enabled = false;
                    btn_work_end.Enabled = true;
                    btn_break_toggle.Enabled = true;

                    btn_break_toggle.Text = _break_time_start;
                    break;
                case WorkStatus.일하는중_쉬는시간없음:
                    //퇴근버튼만 활성화
                    btn_work_start.Enabled = false;
                    btn_work_end.Enabled = true;
                    btn_break_toggle.Enabled = false;

                    btn_break_toggle.Text = _break_time_start;
                    break;
                case WorkStatus.쉬는중:
                    //쉬는시간버튼만 활성화(버튼글자변경)
                    btn_work_start.Enabled = false;
                    btn_work_end.Enabled = false;
                    btn_break_toggle.Enabled = true;

                    btn_break_toggle.Text = _break_time_end;
                    break;
                case WorkStatus.퇴근:
                    //출근해야합니다. 출근버튼만 활성화
                    btn_work_start.Enabled = true;
                    btn_work_end.Enabled = false;
                    btn_break_toggle.Enabled = false;
                    break;
                default:
                    break;
            }

            lb_message.Text = _time_clock_data.work_status.ToString();
        }

        private List<object> _api_work_status = new List<object>();
        private void SetWorkStatus(WorkStatus e_work_status, bool b_is_execute_api)
        {
            _time_clock_data.work_status = e_work_status;
            ChangeButtonStatusByWorkStatus();

            //필요하다면 상태변경과 동시에 api로 상태를 변경합니다
            if(b_is_execute_api)
            {
                _api_work_status.Clear();
                _api_work_status.Add(e_work_status.ToString());
                InsertData("F" + _time_clock_data.access_row.ToString(), ref _api_work_status);
            }
        }

        private void JsonSave()
        {
            //Json저장. Json에 데이터 입력
            JObject json_obj = new JObject
                (
                new JProperty("access_row", _time_clock_data.access_row),
                new JProperty("access_date", _time_clock_data.access_date),
                new JProperty("time_work_total", _time_clock_data.time_work_total),
                new JProperty("time_work_start", _time_clock_data.time_work_start),
                new JProperty("time_work_end", _time_clock_data.time_work_end),
                new JProperty("work_status", _time_clock_data.work_status.ToString()),
                new JProperty("break_time_count", _time_clock_data.break_time_count)
                );

            for (int i = 1; i <= __MAX_BREAK_TIME; ++i)
            {
                json_obj.Add("time_break_time_"+i.ToString(), _time_clock_data.break_times[i - 1]);
            }
            

            //파일로 저장
            File.WriteAllText(_data_time_clock, json_obj.ToString());
        }

        
        private SheetsService _service = null;
        private void DoCredential()
        {
            // 데이터의 수정,추가를 위해서 SheetsService.Scope.Spreadsheets 해준다.
            string[] arr_scope = { SheetsService.Scope.Spreadsheets };

            UserCredential credential;

            // Client 토큰 생성
            using (var stream = new FileStream(_data_client, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    arr_scope,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_data_token_folder, true)).Result;
            }

            // API 서비스 생성
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TimeClock"
            });

            _is_credential = true;
        }

        private void SelectData(string str_column_and_row, out IList<IList<Object>> out_data)
        {
            if (!_is_credential)
                DoCredential();

            var request = _service.Spreadsheets.Values.Get(_sheet_id, _sheet_name + "!" + str_column_and_row);

            ValueRange response = request.Execute();
            out_data = response.Values;
        }

        private void InsertData(string str_sheet_range, ref List<object> list_data)
        {
            if (!_is_credential)
                DoCredential();

            var valueRange = new ValueRange()
            {
                MajorDimension = "ROWS",                    // ROWS or COLUMNS
                Values = new List<IList<object>> { list_data } // 추가할 데이터
            };

            var update = _service.Spreadsheets.Values.Update(valueRange, _sheet_id, str_sheet_range);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            update.Execute();
        }
    }
}
