using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Newtonsoft.Json.Converters;

namespace insurance.query.Controllers
{
    public class IndexController : Controller
    {
        private query_entities db_context = null;

        public FileResult Index()
        {
            return base.File("~/index.html", "text/html");
        }

        public string count_substract_info(DateTime? start_time, DateTime? end_time)
        {
            try
            {
                db_context = new query_entities();
                var list = (from t in this.db_context.CASEOPER
                            where (((t.AUDITSTATUS == 10) && (t.ADState == 3)) && (start_time <= t.REVIEWDATE)) && (t.REVIEWDATE <= end_time)
                            select new { substract_amount = t.SUBTRACTAMOUNT, substract_reason = t.SUBTRACTREASON }).ToList();
                decimal? nullable = 0M;
                string str2 = "";
                for (int i = 0; i < list.Count; i++)
                {
                    decimal? nullable2;
                    if (list[i].substract_amount.HasValue && (((nullable2 = list[i].substract_amount).GetValueOrDefault() > 0M) && nullable2.HasValue))
                    {
                        nullable += list[i].substract_amount;
                    }
                    if ((list[i].substract_reason != null) && (list[i].substract_reason.Trim().Length > 0))
                    {
                        str2 = str2 + list[i].substract_reason + "/n";
                    }
                }
                if (str2.Length > 0)
                {
                    str2 = str2.Substring(0, str2.Length - 2);
                }
                return string.Concat(new object[] { "{\"substract_amount\":", nullable, ",\"substract_reason\":\"", str2, "\"}" });
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string edit_monthly_data(DateTime? start_time, DateTime? end_time)
        {
            try
            {
                db_context = new query_entities();
                return JsonConvert.SerializeObject((from t in this.db_context.TB0014
                                                    where (start_time <= t.AAE031) && (t.AAE031 <= end_time)
                                                    select t).ToList<TB0014>(), new JsonConverter[] { new ChinaDateTimeConverter() });
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string save_case(string case_str, bool is_submit)
        {
            try
            {
                db_context = new query_entities();
                CASEOPER _case = JsonConvert.DeserializeObject<CASEOPER>(case_str);
                if (_case.ID == 0)
                {
                    this.db_context.CASEOPER.AddObject(_case);
                    this.db_context.SaveChanges();
                    return _case.ID.ToString();
                }
                CASEOPER caseoper = (from t in this.db_context.CASEOPER
                                     where t.ID == _case.ID
                                     select t).SingleOrDefault<CASEOPER>();
                if (caseoper != null)
                {
                    caseoper.PNAME = _case.PNAME;
                    caseoper.IDCARD = _case.IDCARD;
                    caseoper.LEAVEDATE = _case.LEAVEDATE;
                    caseoper.PAYAMOUNT = _case.PAYAMOUNT;
                    caseoper.SUBTRACTAMOUNT = _case.SUBTRACTAMOUNT;
                    caseoper.SUBTRACTREASON = _case.SUBTRACTREASON;
                    if (is_submit)
                    {
                        int num = int.Parse(base.Request.Cookies["authority_level"].Value);
                        string str2 = HttpUtility.UrlDecode(base.Request.Cookies["username"].Value);
                        switch (num)
                        {
                            case 1:
                                caseoper.TRIALER = str2;
                                caseoper.TRIALDATE = new DateTime?(DateTime.Now);
                                caseoper.AUDITSTATUS = 9;
                                caseoper.ADState = 1;
                                break;

                            case 2:
                                caseoper.ADState = 7;
                                break;

                            case 9:
                                caseoper.AUDITSTATUS = 10;
                                caseoper.ADState = 3;
                                break;

                            case 10:
                                caseoper.AUDITSTATUS = 2;
                                caseoper.ADState = 5;
                                break;
                        }
                        if (num > 1)
                        {
                            caseoper.REVIEWER = str2;
                            caseoper.REVIEWDATE = new DateTime?(DateTime.Now);
                        }
                    }
                }
                this.db_context.SaveChanges();
                return "1";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string save_monthly_data(string tb0016, bool is_submit)
        {
            string message = "";
            try
            {
                db_context = new query_entities();
                TB0016 t16 = JsonConvert.DeserializeObject<TB0016>(tb0016);
                if (t16.ID == 0)
                {
                    this.db_context.TB0016.AddObject(t16);
                    this.db_context.SaveChanges();
                    message = t16.ID.ToString();
                }
                else
                {
                    TB0016 tb = (from t in this.db_context.TB0016
                                 where t.ID == t16.ID
                                 select t).SingleOrDefault<TB0016>();
                    if (tb != null)
                    {
                        tb.AKB144 = t16.AKB144;
                        tb.AKB147 = t16.AKB147;
                        if (is_submit)
                        {
                            int num = int.Parse(base.Request.Cookies["authority_level"].Value);
                            string str2 = HttpUtility.UrlDecode(base.Request.Cookies["username"].Value);
                            switch (num)
                            {
                                case 1:
                                    tb.AAE026 = str2;
                                    tb.AAE025 = new DateTime?(DateTime.Now);
                                    tb.AAE117 = 9;
                                    tb.ADState = 1;
                                    break;

                                case 2:
                                    tb.AAE032 = str2;
                                    tb.ADState = 7;
                                    break;

                                case 9:
                                    tb.AAE093 = str2;
                                    tb.AAE092 = new DateTime?(DateTime.Now);
                                    tb.AAE117 = 10;
                                    tb.ADState = 3;
                                    break;

                                case 10:
                                    tb.AAE095 = str2;
                                    tb.AAE094 = new DateTime?(DateTime.Now);
                                    tb.AAE117 = 2;
                                    tb.ADState = 5;
                                    break;
                            }
                        }
                    }
                    this.db_context.SaveChanges();
                    message = "1";
                }
                if (is_submit && (int.Parse(base.Request.Cookies["authority_level"].Value) == 2))
                {
                }
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            return message;
        }

        public string search_case(int page_index, int page_size, string id_card, bool is_all)
        {
            string message = "";
            try
            {
                db_context = new query_entities();
                List<CASEOPER> source = null;
                if (!string.IsNullOrEmpty(id_card))
                {
                    source = (from t in this.db_context.CASEOPER
                              where t.IDCARD.StartsWith(id_card)
                              select t).ToList<CASEOPER>();
                }
                else
                {
                    source = this.db_context.CASEOPER.ToList<CASEOPER>();
                }
                int num = int.Parse(base.Request.Cookies["authority_level"].Value);
                if (!is_all)
                {
                    switch (num)
                    {
                        case 1:
                            source = source.Where<CASEOPER>(delegate(CASEOPER t)
                            {
                                int? nullable;
                                return ((t.ADState == 0) || (((nullable = t.ADState).GetValueOrDefault() == 2) && nullable.HasValue));
                            }).ToList<CASEOPER>();
                            break;

                        case 2:
                            source = (from t in source
                                      where t.ADState == 5
                                      select t).ToList<CASEOPER>();
                            break;

                        case 9:
                            source = source.Where<CASEOPER>(delegate(CASEOPER t)
                            {
                                int? nullable;
                                return ((t.ADState == 1) || (((nullable = t.ADState).GetValueOrDefault() == 4) && nullable.HasValue));
                            }).ToList<CASEOPER>();
                            break;

                        case 10:
                            source = source.Where<CASEOPER>(delegate(CASEOPER t)
                            {
                                int? nullable;
                                return ((t.ADState == 3) || (((nullable = t.ADState).GetValueOrDefault() == 6) && nullable.HasValue));
                            }).ToList<CASEOPER>();
                            break;
                    }
                }
                int num2 = source.Count<CASEOPER>();
                int num3 = ((num2 + page_size) - 1) / page_size;
                message = string.Concat(new object[] { "\"page_count\":", num3, ", \"record_count\":", num2, ", \"cases\":" });
                source = (from t in source
                          orderby t.LEAVEDATE descending
                          select t).Skip<CASEOPER>((page_index * page_size)).Take<CASEOPER>(page_size).ToList<CASEOPER>();
                message = message + JsonConvert.SerializeObject(source, new JsonConverter[] { new ChinaDateTimeConverter() });
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            return ("{" + message + "}");
        }

        public string search_monthly_data(int page_index, int page_size, string yjlsh, bool is_all)
        {
            string message = "";
            try
            {
                db_context = new query_entities();
                List<TB0016> source = null;
                if (!string.IsNullOrEmpty(yjlsh))
                {
                    source = (from t in this.db_context.TB0016
                              where t.AAE430.StartsWith(yjlsh)
                              select t).ToList<TB0016>();
                }
                else
                {
                    source = this.db_context.TB0016.ToList<TB0016>();
                }
                if (!is_all)
                {
                    switch (int.Parse(base.Request.Cookies["authority_level"].Value))
                    {
                        case 1:
                            source = source.Where<TB0016>(delegate(TB0016 t)
                            {
                                int? nullable;
                                return ((t.ADState == 0) || (((nullable = t.ADState).GetValueOrDefault() == 2) && nullable.HasValue));
                            }).ToList<TB0016>();
                            break;

                        case 2:
                            source = (from t in source
                                      where t.ADState == 5
                                      select t).ToList<TB0016>();
                            break;

                        case 9:
                            source = source.Where<TB0016>(delegate(TB0016 t)
                            {
                                int? nullable;
                                return ((t.ADState == 1) || (((nullable = t.ADState).GetValueOrDefault() == 4) && nullable.HasValue));
                            }).ToList<TB0016>();
                            break;

                        case 10:
                            source = source.Where<TB0016>(delegate(TB0016 t)
                            {
                                int? nullable;
                                return ((t.ADState == 3) || (((nullable = t.ADState).GetValueOrDefault() == 6) && nullable.HasValue));
                            }).ToList<TB0016>();
                            break;
                    }
                }
                int num2 = source.Count<TB0016>();
                int num3 = ((num2 + page_size) - 1) / page_size;
                message = string.Concat(new object[] { "\"page_count\":", num3, ", \"record_count\":", num2, ", \"cases\":" });
                source = (from t in source
                          orderby t.AAE430 descending
                          select t).Skip<TB0016>((page_index * page_size)).Take<TB0016>(page_size).ToList<TB0016>();
                message = message + JsonConvert.SerializeObject(source, new JsonConverter[] { new ChinaDateTimeConverter() });
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            return ("{" + message + "}");
        }

        public string search_patient(string id_card, DateTime? leave_date)
        {
            string message = "";
            try
            {
                db_context = new query_entities();
                List<patient_info> list = null;
                if (!string.IsNullOrEmpty(id_card))
                {
                    list = (from t11 in this.db_context.TB0011
                            join t12 in this.db_context.TB0012 on new { ID1 = t11.akc190, ID2 = t11.akb020 } equals new { ID1 = t12.AKC190, ID2 = t12.AKB020 }
                            join t01 in this.db_context.TB0001 on t11.AAC002 equals t01.AAC002
                            where t11.AAC002 == id_card
                            select new patient_info { PNAME = t01.AAC003, IDCARD = t11.AAC002, LEAVEDATE = t12.AKC194, PAYAMOUNT = ((t11.ckc281 + t11.ckc282) + t11.ckc283) + t11.ckc285 }).ToList<patient_info>();
                }
                else if (leave_date.HasValue)
                {
                    list = (from t11 in this.db_context.TB0011
                            join t12 in this.db_context.TB0012 on new { ID1 = t11.akc190, ID2 = t11.akb020 } equals new { ID1 = t12.AKC190, ID2 = t12.AKB020 }
                            join t01 in this.db_context.TB0001 on t11.AAC002 equals t01.AAC002
                            where t12.AKC194 == leave_date
                            select new patient_info { PNAME = t01.AAC003, IDCARD = t11.AAC002, LEAVEDATE = t12.AKC194, PAYAMOUNT = ((t11.ckc281 + t11.ckc282) + t11.ckc283) + t11.ckc285 }).ToList<patient_info>();
                }
                return JsonConvert.SerializeObject(list, new JsonConverter[] { new ChinaDateTimeConverter() });
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            return message;
        }

        public string untread_case(int id)
        {
            string str = "";
            try
            {
                db_context = new query_entities();
                CASEOPER caseoper = (from t in this.db_context.CASEOPER
                                     where t.ID == id
                                     select t).SingleOrDefault<CASEOPER>();
                if (caseoper == null)
                {
                    return str;
                }
                switch (int.Parse(base.Request.Cookies["authority_level"].Value))
                {
                    case 9:
                        caseoper.AUDITSTATUS = 1;
                        caseoper.ADState = 2;
                        break;

                    case 10:
                        caseoper.AUDITSTATUS = 9;
                        caseoper.ADState = 4;
                        break;

                    case 2:
                        caseoper.AUDITSTATUS = 10;
                        caseoper.ADState = 6;
                        break;
                }
                this.db_context.SaveChanges();
                return "1";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string untread_monthly_data(int id)
        {
            string str = "";
            try
            {
                db_context = new query_entities();
                TB0016 tb = (from t in this.db_context.TB0016
                             where t.ID == id
                             select t).SingleOrDefault<TB0016>();
                if (tb == null)
                {
                    return str;
                }
                switch (int.Parse(base.Request.Cookies["authority_level"].Value))
                {
                    case 9:
                        tb.AAE117 = 1;
                        tb.ADState = 2;
                        break;

                    case 10:
                        tb.AAE117 = 9;
                        tb.ADState = 4;
                        break;

                    case 2:
                        tb.AAE117 = 10;
                        tb.ADState = 6;
                        break;
                }
                this.db_context.SaveChanges();
                return "1";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string validate_user(string username, string password)
        {
            string message = "0";
            try
            {
                db_context = new query_entities();
                LGN lgn = (from t in this.db_context.LGN
                           where (t.user_name == username) && (t.password == password)
                           select t).SingleOrDefault<LGN>();
                if (lgn != null)
                {
                    lgn.last_login_time = new DateTime?(DateTime.Now);
                    this.db_context.SaveChanges();
                    message = "{\"authority_level\":" + lgn.authority_level + "}";
                }
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            return message;
        }

        public string get_summary_list(int page_index, int page_size, DateTime? start_time, DateTime? end_time)
        {
            string message = "";
            List<TB0014> list_14 = new List<TB0014>();
            List<TB0012> list_12 = new List<TB0012>();

            try
            {
                db_context = new query_entities();
                list_14 = db_context.TB0014.Where(t => t.AAE031 >= start_time && t.AAE031 <= end_time).ToList();
                list_12 = db_context.TB0012.Where(t => t.AKC194 >= start_time && t.AKC194 <= end_time).ToList();

                int source_count = list_12.Count();
                int page_count = ((source_count + page_size) - 1) / page_size;
                list_12 = list_12.OrderByDescending(t => t.AKC194).Skip((page_index * page_size)).Take(page_size).ToList();

                message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count +
                          ",\"source\":{\"summary\":" +
                          JsonConvert.SerializeObject(list_14, new JsonConverter[] {new ChinaDateTimeConverter()}) +
                          ",\"list\":" +
                          JsonConvert.SerializeObject(list_12, new JsonConverter[] {new ChinaDateTimeConverter()}) +
                          "}}";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

        public string illness_payment_search(int page_index, int page_size, string name, string id_card)
        {
            string message = "";

            try
            {
                db_context = new query_entities();
                List<TB0011> source = new List<TB0011>();

                if (!string.IsNullOrEmpty(name))
                {
                    List<string> id_card_list =
                        db_context.TB0001.Where(t => t.AAC003.Contains(name)).Select(t => t.AAC003).ToList();
                    source = db_context.TB0011.Where(t => id_card_list.Contains(t.AAC002)).ToList();
                }
                else if (!string.IsNullOrEmpty(id_card))
                {
                    source = db_context.TB0011.Where(t => t.AAC002.StartsWith(id_card)).ToList();
                }

                int source_count = source.Count();
                int page_count = ((source_count + page_size) - 1) / page_size;
                source = source.OrderByDescending(t => t.aae040).Skip((page_index * page_size)).Take(page_size).ToList();

                message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count + ",\"source\":" +
                          JsonConvert.SerializeObject(source, new JsonConverter[] {new ChinaDateTimeConverter()}) + "}";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

        public string search_personnel_info(int page_index, int page_size, string name, string id_card)
        {
            string message = "";

            try
            {
                db_context = new query_entities();
                List<TB0001> source = new List<TB0001>();

                if (!string.IsNullOrEmpty(name))
                {
                    source = db_context.TB0001.Where(t => t.AAC003.Contains(name)).ToList();
                }
                else if (!string.IsNullOrEmpty(id_card))
                {
                    source = db_context.TB0001.Where(t => t.AAC002.StartsWith(id_card)).ToList();
                }

                int source_count = source.Count();
                int page_count = ((source_count + page_size) - 1) / page_size;
                source = source.OrderByDescending(t => t.AAC006).Skip((page_index * page_size)).Take(page_size).ToList();

                message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count + ",\"source\":" +
                          JsonConvert.SerializeObject(source, new JsonConverter[] { new ChinaDateTimeConverter() }) + "}";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }
    }

    public class patient_info
    {
        public string PNAME;

        public string IDCARD;

        [JsonConverter(typeof(ChinaDateTimeConverter))]
        public DateTime? LEAVEDATE;

        public decimal? PAYAMOUNT;
    }

    public class ChinaDateTimeConverter : DateTimeConverterBase
    {
        private static IsoDateTimeConverter dtConverter = new IsoDateTimeConverter
        {
            DateTimeFormat = "yyyy-MM-dd"
        };

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ChinaDateTimeConverter.dtConverter.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ChinaDateTimeConverter.dtConverter.WriteJson(writer, value, serializer);
        }
    }
}
