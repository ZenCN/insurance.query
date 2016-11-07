using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Converters;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

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

        private string query_personal_info(string AAC002, string type) {
            string result = "";

            if (type == "name")
            {
                result = db_context.TB0001.Where(t => t.AAC002 == AAC002).Select(t => t.AAC003).SingleOrDefault();
            }
            else {  //AC041
                result = db_context.TB0001.Where(t => t.AAC002 == AAC002).Select(t => t.AAC041).SingleOrDefault();
            }

            return result;
        }

        public string get_summary_list(int page_index, int page_size, string hospital_id, string area_code, string state, DateTime start_time, DateTime end_time)
        {
            string message = "";
            try
            {
                db_context = new query_entities();
                IQueryable<TB0004> source_t04 = null;

                if (string.IsNullOrEmpty(state))
                {
                    source_t04 =
                        db_context.TB0004.Where(
                            t => t.aae040 >= start_time && t.aae040 <= end_time && t.akb020 == hospital_id && t.aab324 == area_code);
                }
                else
                {
                    source_t04 =
                        db_context.TB0004.Where(
                            t =>
                                t.aae040 >= start_time && t.aae040 <= end_time && t.akb020 == hospital_id &&
                                t.aae117 == state && t.aab324 == area_code);
                }

                var list = from t04 in source_t04
                           join t12 in db_context.TB0012
                           on new { AKB020 = t04.akb020, AKC190 = t04.akc190, AAC001 = t04.aac001 }
                               equals new { AKB020 = t12.AKB020, AKC190 = t12.AKC190, AAC001 = t12.AAC001 }
                               into _group
                           from g in _group.DefaultIfEmpty()
                           join t01 in db_context.TB0001
                               on t04.aac001 equals t01.AAC001
                           select new
                           {
                               AAC003 = t01.AAC003,
                               AAC041 = t01.AAC041,
                               AKC192 = g == null ? null : g.AKC192,
                               AKC194 = g == null ? null : g.AKC194,
                               AKC195 = g == null ? "" : g.AKC195,
                               AKC198 = g == null ? "" : g.AKC198,
                               akc336 = t04.akc336,
                               akc264 = t04.akc264,
                               akc305 = t04.AKC305,
                               akc190 = t04.akc190,
                               akc253 = t04.akc253,
                               akc280 = t04.akc280,
                               disease_cost_limits = "",  //病种费用限额
                               //disease_cost_limits = (t04.akc261 == null ? 0 : t04.akc261) + (t04.akc260 == null ? 0 : t04.akc260)
                               //+ (t04.akc283 == null ? 0 : t04.akc283), //病种费用限额
                               personal_payment = (t04.akc264 == null ? 0 : t04.akc264) - (t04.akc260 == null ? 0 : t04.akc260), //个人支付
                               akc260 = t04.akc260,
                               swap_amount = "", //调剂金额
                               bkc287 = t04.bkc287
                           };

                var summary = new
                {
                    record_count = source_t04.Count(), 
                    hospital_days = source_t04.Sum(t => t.akc336),
                    akc253 = source_t04.Sum(t => t.akc253),
                    akc264 = list.Sum(t => t.akc264),
                    akc305 = list.Sum(t => t.akc305),
                    personal_payment = list.Sum(t => t.personal_payment),
                    akc260 = list.Sum(t => t.akc260),
                    swap_amount = "",  //调剂金额
                    bkc287 = list.Sum(t => t.bkc287)
                };

                int source_count = list.Count();
                int page_count = ((source_count + page_size) - 1) / page_size;
                list = list.OrderByDescending(t => t.AKC194).Skip((page_index * page_size)).Take(page_size);

                message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count +
                          ",\"source\":{\"summary\":" +
                          JsonConvert.SerializeObject(summary) +
                          ",\"list\":" +
                          JsonConvert.SerializeObject(list.ToList(), new JsonConverter[] { new ChinaDateTimeConverter() }) +
                          "}}";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

        public string search_hospital(string name)
        {
            string message = "";
            try
            {
                db_context = new query_entities();
                var list = (from t05 in db_context.TB0005
                    where t05.akb021.Contains(name)
                    select new
                    {
                        name = t05.akb021,
                        id = t05.akb020
                    }).ToList();

                message = JsonConvert.SerializeObject(list);
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
                int source_count = 0;
                int page_count = 0;

                if (!string.IsNullOrEmpty(name))
                {
                    var source = from t01 in db_context.TB0001
                        join t11 in db_context.TB0011
                            on t01.AAC002 equals t11.AAC002
                        where t01.AAC003.Contains(name) && t11.AKC197 == 1
                        select new
                        {
                            AAC003 = t01.AAC003,
                            AAC002 = t01.AAC002,
                            aae040 = t11.aae040,
                            akc268 = t11.akc268,
                            akc269 = t11.akc269,
                            akc264 = t11.akc264,
                            akc261 = t11.akc261,
                            akc260 = t11.akc260,
                            akc283 = t11.akc283,
                            akc259 = t11.akc259,
                            ckc281 = t11.ckc281,
                            ckc282 = t11.ckc282,
                            ckc283 = t11.ckc283,
                            bkc284 = t11.bkc284,
                            ckc285 = t11.ckc285,
                            bkc286 = t11.bkc286
                        };
                    
                    source_count = source.Count();
                    page_count = ((source_count + page_size) - 1) / page_size;
                    var list = source.OrderByDescending(t => t.aae040).Skip((page_index * page_size)).Take(page_size).ToList();

                    message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count + ",\"source\":" +
                          JsonConvert.SerializeObject(list, new JsonConverter[] { new ChinaDateTimeConverter() }) + "}";
                }
                else if (!string.IsNullOrEmpty(id_card))
                {
                    var source = from t01 in db_context.TB0001
                                 join t11 in db_context.TB0011
                                     on t01.AAC002 equals t11.AAC002
                                 where t01.AAC002.StartsWith(id_card) && t11.AKC197 == 1
                                 select new
                                 {
                                     AAC003 = t01.AAC003,
                                     AAC002 = t01.AAC002,
                                     aae040 = t11.aae040,
                                     akc268 = t11.akc268,
                                     akc269 = t11.akc269,
                                     akc264 = t11.akc264,
                                     akc261 = t11.akc261,
                                     akc260 = t11.akc260,
                                     akc283 = t11.akc283,
                                     akc259 = t11.akc259,
                                     ckc281 = t11.ckc281,
                                     ckc282 = t11.ckc282,
                                     ckc283 = t11.ckc283,
                                     bkc284 = t11.bkc284,
                                     ckc285 = t11.ckc285,
                                     bkc286 = t11.bkc286
                                 };

                    source_count = source.Count();
                    page_count = ((source_count + page_size) - 1)/page_size;
                    var list = source.OrderByDescending(t => t.aae040).Skip((page_index*page_size)).Take(page_size).ToList();

                    message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count + ",\"source\":" +
                          JsonConvert.SerializeObject(list, new JsonConverter[] { new ChinaDateTimeConverter() }) + "}";
                }
                else
                {
                    var source = from t11 in db_context.TB0011
                        join t01 in db_context.TB0001
                            on t11.AAC002 equals t01.AAC002 into _group
                        from tb01 in _group.DefaultIfEmpty()
                        where t11.AKC197 == 1
                        select new
                        {
                            AAC003 = tb01 == null ? "" : tb01.AAC003,
                            AAC002 = tb01 == null ? "" : tb01.AAC002,
                            aae040 = t11.aae040,
                            akc268 = t11.akc268,
                            akc269 = t11.akc269,
                            akc264 = t11.akc264,
                            akc261 = t11.akc261,
                            akc260 = t11.akc260,
                            akc283 = t11.akc283,
                            akc259 = t11.akc259,
                            ckc281 = t11.ckc281,
                            ckc282 = t11.ckc282,
                            ckc283 = t11.ckc283,
                            bkc284 = t11.bkc284,
                            ckc285 = t11.ckc285,
                            bkc286 = t11.bkc286
                        };

                    source_count = source.Count();
                    page_count = ((source_count + page_size) - 1) / page_size;
                    var list = source.OrderByDescending(t => t.aae040).Skip((page_index * page_size)).Take(page_size).ToList();

                    message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count + ",\"source\":" +
                          JsonConvert.SerializeObject(list, new JsonConverter[] { new ChinaDateTimeConverter() }) + "}";
                }
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
                List<TB0001> list = new List<TB0001>();
                int source_count = 0;
                int page_count = 0;

                if (!string.IsNullOrEmpty(name))
                {
                    var source = db_context.TB0001.Where(t => t.AAC003.Contains(name));
                    source_count = source.Count();
                    page_count = ((source_count + page_size) - 1) / page_size;
                    list = source.OrderByDescending(t => t.AAC006).Skip((page_index*page_size)).Take(page_size).ToList();
                }
                else if (!string.IsNullOrEmpty(id_card))
                {
                    var source = db_context.TB0001.Where(t => t.AAC002.StartsWith(id_card));
                    source_count = source.Count();
                    page_count = ((source_count + page_size) - 1) / page_size;
                    list = source.OrderByDescending(t => t.AAC006).Skip((page_index*page_size)).Take(page_size).ToList();
                }
                else
                {
                    source_count = db_context.TB0001.Count();
                    page_count = ((source_count + page_size) - 1) / page_size;
                    list = db_context.TB0001.OrderByDescending(t => t.AAC006).Skip((page_index * page_size)).Take(page_size).ToList();
                }

                message = "{\"page_count\":" + page_count + ",\"record_count\":" + source_count + ",\"source\":" +
                          JsonConvert.SerializeObject(list, new JsonConverter[] { new ChinaDateTimeConverter() }) + "}";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

        public void export_to_excel(string hospital_id, string area_code, string state, DateTime start_time, DateTime end_time, string hospital_name)
        {
            db_context = new query_entities();
            IQueryable<TB0004> source_t04 = null;

            if (string.IsNullOrEmpty(state))
            {
                source_t04 =
                    db_context.TB0004.Where(
                        t => t.aae040 >= start_time && t.aae040 <= end_time && t.akb020 == hospital_id && t.aab324 == area_code);
            }
            else
            {
                source_t04 =
                    db_context.TB0004.Where(
                        t =>
                            t.aae040 >= start_time && t.aae040 <= end_time && t.akb020 == hospital_id &&
                            t.aae117 == state && t.aab324 == area_code);
            }

            var list = (from t04 in source_t04
                join t12 in db_context.TB0012
                    on new {AKB020 = t04.akb020, AKC190 = t04.akc190, AAC001 = t04.aac001}
                    equals new {AKB020 = t12.AKB020, AKC190 = t12.AKC190, AAC001 = t12.AAC001}
                    into _group
                from g in _group.DefaultIfEmpty()
                join t01 in db_context.TB0001
                    on t04.aac001 equals t01.AAC001
                select new excel_model
                {
                    AAC003 = t01.AAC003,
                    AAC041 = t01.AAC041,
                    _AKC192 = g == null ? null : g.AKC192,
                    _AKC194 = g == null ? null : g.AKC194,
                    AKC195 = g == null ? "" : g.AKC195,
                    AKC198 = g == null ? "" : g.AKC198,
                    _hospital_days = t04.akc336,
                    _akc264 = t04.akc264,
                    _akc305 = t04.AKC305,
                    akc190 = t04.akc190,
                    _akc253 = t04.akc253,
                    _akc280 = t04.akc280,
                    disease_cost_limits = "", //病种费用限额
                    //disease_cost_limits = (t04.akc261 == null ? 0 : t04.akc261) + (t04.akc260 == null ? 0 : t04.akc260)
                    //+ (t04.akc283 == null ? 0 : t04.akc283), //病种费用限额
                    _personal_payment = (t04.akc264 == null ? 0 : t04.akc264) - (t04.akc260 == null ? 0 : t04.akc260),  //个人支付
                    _akc260 = t04.akc260,
                    swap_amount = "", //调剂金额
                    _bkc287 = t04.bkc287
                }).OrderByDescending(t => t._AKC194).ToList();

            var sum = new excel_model
            {
                record_count = source_t04.Count().ToString(),
                _hospital_days = source_t04.Sum(t => t.akc336),
                _akc253 = source_t04.Sum(t => t.akc253),
                _akc264 = list.Sum(t => t._akc264),
                _akc305 = list.Sum(t => t._akc305),
                _personal_payment = list.Sum(t => t._personal_payment),
                _akc260 = list.Sum(t => t._akc260),
                swap_amount = "",  //调剂金额
                _bkc287 = list.Sum(t => t._bkc287)
            };

            new ExcelOper().export(sum, list, start_time.ToString("yyyy年M月"), hospital_name);
        }
    }

    public class ExcelOper
    {
        public void export(excel_model sum, List<excel_model> list, string date, string hospital_name)
        {
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "excel/model.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
            }

            int strat_row = 6;
            ISheet sheet = hssfworkbook.GetSheet("汇总及清单");
            sheet = CreateCell(sheet, list.Count(), 16, strat_row);

            //日期及医院名称
            sheet.GetRow(0).GetCell(0).SetCellValue("日期：" + date);
            sheet.GetRow(0).GetCell(3).SetCellValue(hospital_name);
            //汇总表
            sheet.GetRow(3).GetCell(0).SetCellValue(sum.record_count);
            sheet.GetRow(3).GetCell(1).SetCellValue(sum.hospital_days);
            sheet.GetRow(3).GetCell(2).SetCellValue(sum.akc264);
            sheet.GetRow(3).GetCell(3).SetCellValue(sum.akc305);
            sheet.GetRow(3).GetCell(4).SetCellValue(sum.personal_payment);
            sheet.GetRow(3).GetCell(5).SetCellValue(sum.akc260);
            sheet.GetRow(3).GetCell(6).SetCellValue(sum.akc253);
            sheet.GetRow(3).GetCell(7).SetCellValue(sum.swap_amount);
            sheet.GetRow(3).GetCell(8).SetCellValue(sum.bkc287);
            //清单表
            ICell cell = null;
            ICellStyle cell_style = SetCellStyle(sheet);
            for (int i = 0; i < list.Count(); i++)
            {
                cell = sheet.GetRow(strat_row + i).GetCell(0);
                cell.SetCellValue(list[i].akc190);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(1);
                cell.SetCellValue(list[i].AAC003);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(2);
                cell.SetCellValue(QueryPersonType(list[i].AAC041.Trim().Length > 0 ? int.Parse(list[i].AAC041) : 0));
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(3);
                cell.SetCellValue(list[i].AKC192);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(4);
                cell.SetCellValue(list[i].AKC194);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(5);
                cell.SetCellValue(list[i].AKC195);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(6);
                cell.SetCellValue(list[i].AKC198);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(7);
                cell.SetCellValue(list[i].hospital_days);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(8);
                cell.SetCellValue(list[i].akc264);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(9);
                cell.SetCellValue(list[i].akc305);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(10);
                cell.SetCellValue(list[i].disease_cost_limits);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(11);
                cell.SetCellValue(list[i].personal_payment);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(12);
                cell.SetCellValue(list[i].akc260);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(13);
                cell.SetCellValue(list[i].akc253);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(14);
                cell.SetCellValue(list[i].akc280);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(15);
                cell.SetCellValue(list[i].swap_amount);
                cell.CellStyle = cell_style;

                cell = sheet.GetRow(strat_row + i).GetCell(16);
                cell.SetCellValue(list[i].bkc287);
                cell.CellStyle = cell_style;
            }

            ResponseExcel(hssfworkbook, date, hospital_name);
        }

        private string QueryPersonType(int val)
        {
            switch (val)
            {
                case 1:
                    return "一般人员";
                case 2:
                    return "低保家庭";
                case 3:
                    return "重度残疾";
                case 4:
                    return "三无人员";
                case 5:
                    return "困难人员";
                default:
                    return "";
            }
        }

        private ISheet CreateCell(ISheet sheet, int row_count, int col_count, int start_row)
        {
            for (int i = 0; i <= row_count; i++)
            {
                IRow row = null;
                if (sheet.GetRow(start_row + i) == null)
                {
                    row = sheet.CreateRow(start_row + i);
                }
                else
                {
                    row = sheet.GetRow(start_row + i);
                }
                for (int j = 0; j <= col_count; j++)
                {
                    if (row.GetCell(j) == null)
                    {
                        row.CreateCell(j);
                    }
                }
            }

            return sheet;
        }

        private ICellStyle SetCellStyle(ISheet sheet)
        {
            ICellStyle style = sheet.Workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;//水平对齐居中
            style.BorderBottom = BorderStyle.Thin; //边框是黑色的
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            return style;
        }

        private void ResponseExcel(HSSFWorkbook hssfworkbook, string date, string hospital_name)
        {
            // 设置响应头（文件名和文件格式）
            //设置响应的类型为Excel
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            //设置下载的Excel文件名
            HttpContext.Current.Response.AddHeader("Content-Disposition",
                string.Format("attachment; filename={0}", date + " " + hospital_name + ".xls"));
            //Clear方法删除所有缓存中的HTML输出。但此方法只删除Response显示输入信息，不删除Response头信息。以免影响导出数据的完整性。
            HttpContext.Current.Response.Clear();

            //写入到客户端
            using (MemoryStream ms = new MemoryStream())
            {
                //将工作簿的内容放到内存流中
                hssfworkbook.Write(ms);
                //将内存流转换成字节数组发送到客户端
                HttpContext.Current.Response.BinaryWrite(ms.GetBuffer());
                //HttpContext.Current.ApplicationInstance.CompleteRequest();//为了解决Response.End()由于代码已经过优化或者本机框架位于调用堆栈之上，无法计算表达式的值 的异常
                HttpContext.Current.Response.End();
            }
        }
    }

    public class excel_model
    {
        public string hospital_days
        {
            get { return _hospital_days == null ? "" : _hospital_days.ToString(); }
        }

        public decimal? _hospital_days;

        public string akc264
        {
            get { return _akc264 == null ? "" : _akc264.ToString(); }
        }

        public decimal? _akc264;

        public string akc305
        {
            get { return _akc305 == null ? "" : _akc305.ToString(); }
        }

        public decimal? _akc305;

        public string personal_payment
        {
            get { return _personal_payment == null ? "" : _personal_payment.ToString(); }
        }

        public decimal? _personal_payment;

        public string akc260
        {
            get { return _akc260 == null ? "" : _akc260.ToString(); }
        }

        public decimal? _akc260;

        public string akc253
        {
            get { return _akc253 == null ? "" : _akc253.ToString(); }
        }

        public decimal? _akc253;

        public string bkc287
        {
            get { return _bkc287 == null ? "" : _bkc287.ToString(); }
        }

        public decimal? _bkc287;

        public string AKC192
        {
            get { return _AKC192 == null ? "" : Convert.ToDateTime(_AKC192).ToString("yyyy年M月"); }
        }

        public DateTime? _AKC192;

        public string AKC194
        {
            get { return _AKC194 == null ? "" : Convert.ToDateTime(_AKC194).ToString("yyyy年M月"); }
        }

        public DateTime? _AKC194;

        public string akc280
        {
            get { return _akc280 == null ? "" : _akc280.ToString(); }
        }

        public decimal? _akc280;

        public string record_count;
        public string akc190;
        public string AAC003;
        public string AAC041;
        public string swap_amount;
        public string AKC195;
        public string AKC198;
        public string disease_cost_limits;
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
