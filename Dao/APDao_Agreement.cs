using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace OrderToolWebservers
{
    public class APDao_Agreement:MySqlHelper
    {
        public SerializableDictionary<string, string> getCbContent(string typecode)
        {
            SerializableDictionary<string, string> info = new SerializableDictionary<string, string>();
            string sql = "select * from SYS_CODE where typecode=:typecode and ownerid=:ownerid order by name desc";
            OracleCommand selCmd = null;
            OracleTransaction trans = null;
            try
            {
                selCmd = Conn_datacenter_cmszh.CreateCommand();
                selCmd.Connection = Conn_datacenter_cmszh;
                selCmd.CommandType = System.Data.CommandType.Text;
                selCmd.CommandText = sql.ToString();
                selCmd.CommandTimeout = 3000;
                selCmd.Parameters.Add("typecode", typecode);
                //-------2019-2-12-
                selCmd.Parameters.Add("ownerid", Properties.Settings.Default.OWNERID);
                OracleDataReader res = selCmd.ExecuteReader();
                while (res.Read())
                {
                    string Name = res["NAME"].ToString().Trim();
                    string Code = res["CODE"].ToString().Trim();
                    info.Add(Code, Name);
                }
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.ToString(), "错误信息");

            }
            finally
            {
                if (selCmd != null)
                    selCmd.Dispose();

                if (trans != null)
                    trans.Dispose();
            }

            return info;
        }

        public SPRetInfos UpdateColValues(string Empid, string colName, string colValue, string agreementId, int roleType)
        {
            OracleCommand spCmd = null;
            OracleTransaction trans = null;
            SPRetInfos retinfo = new SPRetInfos();
            int retCode = -1;
            try
            {
                trans = Conn_datacenter_cmszh.BeginTransaction();
                spCmd = Conn_datacenter_cmszh.CreateCommand();
                spCmd.Connection = Conn_datacenter_cmszh;
                spCmd.CommandType = System.Data.CommandType.StoredProcedure;
                spCmd.CommandText = "PG_AGR_CSTINFO.P_AGR_COLUPDATE";
                spCmd.CommandTimeout = 1800;
                OracleParameter[] parameters ={

                    new OracleParameter("IN_COLNAME",OracleDbType.Varchar2,ParameterDirection.Input),
                    new OracleParameter("IN_COLVALUE",OracleDbType.Varchar2,ParameterDirection.Input),
                    new OracleParameter("IN_AGREEMENT_ID",OracleDbType.Int64,ParameterDirection.Input),
                    new OracleParameter("IN_EMPID",OracleDbType.Int64,ParameterDirection.Input),
                    new OracleParameter("IN_ROLETYPE",OracleDbType.Int64,ParameterDirection.Input),


                    new OracleParameter("OUT_CODE",OracleDbType.Int64,ParameterDirection.Output),
                    new OracleParameter("OUT_MSG",OracleDbType.Varchar2,ParameterDirection.Output)

                };
                parameters[0].Value = colName;
                parameters[1].Value = colValue;
                parameters[2].Value = Int64.Parse(agreementId);
                parameters[3].Value = Empid;
                parameters[4].Value = roleType;
                //parameters[1].Value = userinfo.empid.ToString();
                /*Oracle For .Net has Bug: OUTPUT parameter must set size like the following satetment!!!!!*/
                parameters[5].Size = 8;
                parameters[6].Size = 2048;
                foreach (OracleParameter parameter in parameters)
                {
                    spCmd.Parameters.Add(parameter);
                }


                int ret = spCmd.ExecuteNonQuery();
                retinfo.num = parameters[5].Value.ToString().Trim();
                retinfo.msg = parameters[6].Value.ToString().Trim();
                //retinfo.result = parameters[1].Value.ToString().Trim();
                spCmd.Dispose();
                spCmd = null;
                retCode = 0;

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString(), "错误信息");
                if (trans != null)
                    trans.Rollback();
                retCode = -1;
            }
            finally
            {
                if (trans != null)
                    trans.Dispose();

                if (spCmd != null)
                    spCmd.Dispose();
            }

            return retinfo;

        }

        //销售反馈 查询(勾选时间)
        public SortableBindingList<SaleFeed> GetSaleFeedBackInfos( string Emproleid,string Empname, SerializableDictionary<string, string> sqlkeydict, string time)
        {
            SortableBindingList<SaleFeed> infolist = new SortableBindingList<SaleFeed>();
            string sql = "";
            if (Emproleid == "117" || Emproleid == "104")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where 1=1 and saller=:saller and begindate=:begindate and compid=:compid and ownerid=:ownerid $";
            }
            if (Emproleid == "118" || Emproleid == "123")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where (sallmanager=:sallmanager or sallmanager is null or saller is null or sallleader is null) and begindate=:begindate and compid=:compid and ownerid=:ownerid $";
            }
            if (Emproleid == "119")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where 1=1 and sallLeader=:sallLeader and begindate=:begindate and compid=:compid and ownerid=:ownerid $";
            }
            if (Emproleid == "99" || Emproleid == "120" || Emproleid == "121")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where 1=1 and begindate=:begindate and compid=:compid and ownerid=:ownerid $";
            }
            OracleCommand cmd = null;
            OracleTransaction trans = null;

            try
            {
                cmd = Conn_datacenter_cmszh.CreateCommand();
                cmd.Connection = Conn_datacenter_cmszh;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql.ToString();
                if (Emproleid == "117" || Emproleid == "104")
                {
                    cmd.Parameters.Add("saller", Empname);
                }
                if (Emproleid == "118"||Emproleid == "123")
                {
                    cmd.Parameters.Add("sallmanager", Empname);
                }
                if (Emproleid == "119")
                {
                    cmd.Parameters.Add("sallLeader", Empname);
                }
                cmd.Parameters.Add("begindate", DateTime.ParseExact(time, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture));
                cmd.Parameters.Add("compid", Properties.Settings.Default.COMPID);
                cmd.Parameters.Add("ownerid", Properties.Settings.Default.OWNERID);
                cmd.CommandTimeout = 1800;

                string whereStr = "";
                //遍历Dictionary中的key值
                foreach (KeyValuePair<string, string> kv in sqlkeydict)
                {
                    if (kv.Key.Equals(null) || kv.Key.Equals(""))
                    {
                        whereStr = whereStr + "and" + kv.Key.Replace("%", "") + "=" + kv.Key.Replace("%", "");
                    }
                    else
                    {
                        if (kv.Key.IndexOf("%") > -1)
                        {
                            whereStr = whereStr + " and " + kv.Key.Replace("%", "") + " like:" + kv.Key.Replace("%", "");

                        }
                        else
                        {
                            whereStr = whereStr + " and " + kv.Key + "=:" + kv.Key;
                        }

                    }

                }
                sql = sql.Replace("$", whereStr);

                cmd.CommandText = sql.ToString();
                //遍历Dictionary中的value值
                foreach (KeyValuePair<string, string> kv in sqlkeydict)
                {
                    if (!kv.Key.Equals(null) && !kv.Key.Equals(""))
                    {
                        if (kv.Key.IndexOf("%") > -1)
                        {
                            cmd.Parameters.Add(kv.Key.Replace("%", ""), kv.Value);
                        }
                        else
                        {
                            cmd.Parameters.Add(kv.Key, kv.Value);
                        }
                    }
                }
                string s = sql;
                OracleDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    SaleFeed infos = new SaleFeed();
                    infos.YearNum = res["YEARNUM"].ToString().Trim();
                    infos.AgreementId = res["AGREEMENT_ID"].ToString().Trim();
                    infos.ProdId = res["PROD_ID"].ToString().Trim();
                    infos.ProdCode = res["PROD_CODE"].ToString().Trim();
                    infos.ProdName = res["PROD_NAME"].ToString().Trim();
                    infos.TarGet = res["TARGETNAME"].ToString().Trim();
                    infos.CstCode = res["CSTCODE"].ToString().Trim();
                    infos.CstName = res["CSTNAME"].ToString().Trim();
                    infos.SallLeader = res["SALLLEADER"].ToString().Trim();
                    infos.SallManager = res["SALLMANAGER"].ToString().Trim();
                    infos.Saller = res["SALLER"].ToString().Trim();
                    infos.CstIntention = res["CSTINTENTIONNAME"].ToString().Trim();
                    infos.ProdIntention = res["PRODINTENTIONNAME"].ToString().Trim();
                    infos.Mark = res["MARK"].ToString().Trim();
                    infos.Dynamics = res["DYNAMICSNAME"].ToString().Trim();
                    infos.FinalChannel = res["FINALCHANNELNAME"].ToString().Trim();
                    infos.ThisYearValues = res["THISYEARVALUES"].ToString().Trim();
                    infos.HopeValues = res["HOPEVALUES"].ToString().Trim();
                    infos.Import = res["IMPORTNAME"].ToString().Trim();
                    infos.BuyerName = res["BUYERNAMENAME"].ToString().Trim();
                    infos.Manager = res["MANAGERNAME"].ToString().Trim();
                    infos.AgreeType = res["AGREETYPENAME"].ToString().Trim();
                    infos.MiddleMan = res["MIDDLEMAN"].ToString().Trim();
                    infos.AgreeLevel = res["AGREELEVELNAME"].ToString().Trim();
                    infos.LastValues = res["LASTVALUES"].ToString().Trim();
                    infos.LastUpStream = res["LASTUPSTREAMNAME"].ToString().Trim();
                    infos.ForecastValues = res["FORECASTVALUES"].ToString().Trim();
                    infos.BeginDate = res["BEGINDATE"].ToString().Trim();
                    infos.Seal = res["SEALNAME"].ToString().Trim();
                    infos.Onfile = res["ONFILENAME"].ToString().Trim();
                    infos.FinalValues = res["FINALVALUES"].ToString().Trim();
                    infos.ModifyBuyerTime = res["MODIFYBUYERTIME"].ToString().Trim();

                    infolist.Add(infos);
                }

            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.ToString());

            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();

                if (trans != null)
                    trans.Dispose();

            }
            return infolist;
        }

        public SortableBindingList<SaleFeed> GetSaleFeedBackInfo(string Emproleid, string Empname, SerializableDictionary<string, string> sqlkeydict)
        {
            SortableBindingList<SaleFeed> infolist = new SortableBindingList<SaleFeed>();
            string sql = "";
            if (Emproleid == "117" || Emproleid == "104")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where 1=1 and saller=:saller and compid=:compid and ownerid=:ownerid $";
            }
            if (Emproleid == "118" || Emproleid == "123")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where (sallmanager=:sallmanager or sallmanager is null or saller is null or sallleader is null) and compid=:compid and ownerid=:ownerid $";
            }
            if (Emproleid == "119")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where 1=1 and sallLeader=:sallLeader and compid=:compid and ownerid=:ownerid $";
            }
            if (Emproleid == "99" || Emproleid == "120" || Emproleid == "121")
            {
                sql = "select * from V_AGREEMENT_CLIENT_INFO where compid=:compid and ownerid=:ownerid $";
            }
            OracleCommand cmd = null;
            OracleTransaction trans = null;

            try
            {
                cmd = Conn_datacenter_cmszh.CreateCommand();
                cmd.Connection = Conn_datacenter_cmszh;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql.ToString();
                if (Emproleid == "117" || Emproleid == "104")
                {
                    cmd.Parameters.Add("saller", Empname);
                }
                if (Emproleid == "118" || Emproleid == "123")
                {
                    cmd.Parameters.Add("sallmanager", Empname);
                }
                if (Emproleid == "119")
                {
                    cmd.Parameters.Add("sallLeader", Empname);
                }
                cmd.Parameters.Add("compid", Properties.Settings.Default.COMPID);
                cmd.Parameters.Add("ownerid", Properties.Settings.Default.OWNERID);
                cmd.CommandTimeout = 1800;

                string whereStr = "";
                //遍历Dictionary中的key值
                foreach (KeyValuePair<string, string> kv in sqlkeydict)
                {
                    if (kv.Key.Equals(null) || kv.Key.Equals(""))
                    {
                        whereStr = whereStr + "and" + kv.Key.Replace("%", "") + "=" + kv.Key.Replace("%", "");
                    }
                    else
                    {
                        if (kv.Key.IndexOf("%") > -1)
                        {
                            whereStr = whereStr + " and " + kv.Key.Replace("%", "") + " like:" + kv.Key.Replace("%", "");

                        }
                        else
                        {
                            whereStr = whereStr + " and " + kv.Key + "=:" + kv.Key;
                        }

                    }

                }
                sql = sql.Replace("$", whereStr);

                cmd.CommandText = sql.ToString();
                //遍历Dictionary中的value值
                foreach (KeyValuePair<string, string> kv in sqlkeydict)
                {
                    if (!kv.Key.Equals(null) && !kv.Key.Equals(""))
                    {
                        if (kv.Key.IndexOf("%") > -1)
                        {
                            cmd.Parameters.Add(kv.Key.Replace("%", ""), kv.Value);
                        }
                        else
                        {
                            cmd.Parameters.Add(kv.Key, kv.Value);
                        }
                    }
                }
                string s = sql;
                OracleDataReader res = cmd.ExecuteReader();
                while (res.Read())
                {
                    SaleFeed infos = new SaleFeed();
                    infos.YearNum = res["YEARNUM"].ToString().Trim();
                    infos.AgreementId = res["AGREEMENT_ID"].ToString().Trim();
                    infos.ProdId = res["PROD_ID"].ToString().Trim();
                    infos.ProdCode = res["PROD_CODE"].ToString().Trim();
                    infos.ProdName = res["PROD_NAME"].ToString().Trim();
                    infos.TarGet = res["TARGETNAME"].ToString().Trim();
                    infos.CstCode = res["CSTCODE"].ToString().Trim();
                    infos.CstName = res["CSTNAME"].ToString().Trim();
                    infos.SallLeader = res["SALLLEADER"].ToString().Trim();
                    infos.SallManager = res["SALLMANAGER"].ToString().Trim();
                    infos.Saller = res["SALLER"].ToString().Trim();
                    infos.CstIntention = res["CSTINTENTIONNAME"].ToString().Trim();
                    infos.ProdIntention = res["PRODINTENTIONNAME"].ToString().Trim();
                    infos.Mark = res["MARK"].ToString().Trim();
                    infos.Dynamics = res["DYNAMICSNAME"].ToString().Trim();
                    infos.FinalChannel = res["FINALCHANNELNAME"].ToString().Trim();
                    infos.ThisYearValues = res["THISYEARVALUES"].ToString().Trim();
                    infos.HopeValues = res["HOPEVALUES"].ToString().Trim();
                    infos.Import = res["IMPORTNAME"].ToString().Trim();
                    infos.BuyerName = res["BUYERNAMENAME"].ToString().Trim();
                    infos.Manager = res["MANAGERNAME"].ToString().Trim();
                    infos.AgreeType = res["AGREETYPENAME"].ToString().Trim();
                    infos.MiddleMan = res["MIDDLEMAN"].ToString().Trim();
                    infos.AgreeLevel = res["AGREELEVELNAME"].ToString().Trim();
                    infos.LastValues = res["LASTVALUES"].ToString().Trim();
                    infos.LastUpStream = res["LASTUPSTREAMNAME"].ToString().Trim();
                    infos.ForecastValues = res["FORECASTVALUES"].ToString().Trim();
                    infos.BeginDate = res["BEGINDATE"].ToString().Trim();
                    infos.Seal = res["SEALNAME"].ToString().Trim();
                    infos.Onfile = res["ONFILENAME"].ToString().Trim();
                    infos.FinalValues = res["FINALVALUES"].ToString().Trim();
                    infos.ModifyBuyerTime = res["MODIFYBUYERTIME"].ToString().Trim();

                    infolist.Add(infos);
                }

            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.ToString());

            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();

                if (trans != null)
                    trans.Dispose();

            }
            return infolist;
        }

        //获取员工信息
        public SortableBindingList<EmpInfos> GetEmpInfo(SerializableDictionary<string, string> sqlkeydict)
        {

            SortableBindingList<EmpInfos> infoList = new SortableBindingList<EmpInfos>();
            string selSql = "select * from v_pub_emp where 1=1$";
            OracleCommand selCmd = null;
            OracleTransaction trans = null;
            try
            {
                selCmd = Conn_datacenter_cmszh.CreateCommand();
                selCmd.Connection = Conn_datacenter_cmszh;
                selCmd.CommandType = System.Data.CommandType.Text;
                selCmd.CommandText = selSql.ToString();

                string whereStr = "";
                //遍历Dictionary中的key值
                foreach (KeyValuePair<string, string> kv in sqlkeydict)
                {
                    if (kv.Key.Equals(null) || kv.Key.Equals(""))
                    {
                        whereStr = whereStr + "and" + kv.Key.Replace("%", "") + "=" + kv.Key.Replace("%", "");
                    }
                    else
                    {
                        if (kv.Key.IndexOf("%") > -1)
                        {
                            whereStr = whereStr + " and " + kv.Key.Replace("%", "") + " like:" + kv.Key.Replace("%", "");

                        }
                        else
                        {
                            whereStr = whereStr + " and " + kv.Key + "=:" + kv.Key;
                        }

                    }

                }
                selSql = selSql.Replace("$", whereStr);

                selCmd.CommandText = selSql.ToString();
                //遍历Dictionary中的value值
                foreach (KeyValuePair<string, string> kv in sqlkeydict)
                {
                    if (!kv.Key.Equals(null) && !kv.Key.Equals(""))
                    {
                        if (kv.Key.IndexOf("%") > -1)
                        {
                            selCmd.Parameters.Add(kv.Key.Replace("%", ""), kv.Value);
                        }
                        else
                        {
                            selCmd.Parameters.Add(kv.Key, kv.Value);
                        }
                    }
                }
                string s = selSql;
                OracleDataReader res = selCmd.ExecuteReader();
                while (res.Read())
                {
                    EmpInfos infos = new EmpInfos();
                    infos.EmpCode = res["EMPCODE"].ToString().Trim();
                    infos.EmpName = res["EMPNAME"].ToString().Trim();
                    infos.EmpId = res["EMPID"].ToString().Trim();
                    //infos.BeginDate = res["BEGINDATE"].ToString().Trim();
                    infoList.Add(infos);
                }

            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.ToString(), "错误信息");
                if (trans != null)
                {
                    trans.Rollback();
                }
            }
            finally
            {
                if (selCmd != null)
                {
                    selCmd.Dispose();
                }
            }
            return infoList;
        }

    }
}