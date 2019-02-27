using MySql.Data.MySqlClient;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml.Serialization;

namespace OrderToolWebservers
{
    /// <summary>
    /// WebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
        [WebMethod(Description = "连接mysql主数据库")]
        public int getDBConnect() {
            return MySqlHelper.ConnectionTest();
        }

        [WebMethod(Description = "连接mysql从库")]
        public int getMysqlDB()
        {
            MySqlHelper mysql = new MySqlHelper();
            return mysql.ConnectionTests();
        }

        [WebMethod(Description = "连接oracle数据库")]
        public int getOrclDB()
        {
            MySqlHelper mysql = new MySqlHelper();
            return mysql.ConnectionTest_cmszh();          
        }

        [WebMethod(Description ="PingDB")]
        public  void PingDB()
        {
            MySqlHelper.PingDB();         
        }
        [WebMethod(Description = "PingALLDB")]
        public int pingAllDB()
        {
            return MySqlHelper.pingAllDB();
        }
        [WebMethod(Description = "CloseALLDB")]
        public void closeAllDB()
        {
             MySqlHelper.closeAllDB();
        }
        [WebMethod(Description = "CloseDB")]
        public void CloseDB()
        {
            MySqlHelper.CloseDB();
        }
        [WebMethod(Description = "GetDbBase")]
        public string GetDBbase()
        {
            return MySqlHelper.DBTYPE;
        }
        [WebMethod (Description ="查询规则表")]
        public SessionDtos CheckLoginUser(string version, string user, string passwords, string deptId)
        {
            PMSystemDao dao = new PMSystemDao();
            return dao.CheckLoginUser(version,user,passwords,deptId);
        }     
        [XmlInclude(typeof(PubOwnerConfigureDto))]
        [WebMethod(Description = "查询货主规则配置表")]
        public int GetPubOwnerConfigureInfo(string deptid) {
            PMSystemDao dao = new PMSystemDao();
            return dao.GetPubOwnerConfigureInfo(deptid);
        }
        [WebMethod(Description ="查看部门规则配置表")]
        public int GetPubDeptConfigureInfo(string deptid)
        {
            PMSystemDao dao = new PMSystemDao();
            return dao.GetPubDeptConfigureInfo(deptid);

        }

        [WebMethod (Description ="获取所有部门")]
        public DataTable GetAllDept(string flag)
        {
            PMSystemDao dao = new PMSystemDao();
            DataTable table = new DataTable();
            table.TableName = "tmp";
            table = dao.GetAllDept(flag);
            return table;
        }
        [WebMethod(Description = "获取菜单")]
        public DataTable GetMenuByRoleId(string roleId) {
            PMSystemDao dao = new PMSystemDao();
            return dao.GetMenuByRoleId(roleId);

        }
        [WebMethod(Description = "获取所有菜单")]
        public DataTable GetAllMenu(string stopFlag)
        {
            PMSystemDao dao = new PMSystemDao();
            return dao.GetAllMenu(stopFlag);
        }

        [WebMethod(Description = "修改密码")]
        public string[] ChangePassword(string empId, string oldPwd, string newPwd)
        {
            PMSystemDao dao = new PMSystemDao();
           return dao.ChangePassword(empId, oldPwd, newPwd);
        }

        [WebMethod(Description = "获取下拉框")]
        public SerializableDictionary<string, string> getCbContent(string typecode)
        {
            APDao_Agreement dao = new APDao_Agreement();
            return dao.getCbContent(typecode);
        }
        [WebMethod(Description = "单个字段修改")]
        public SPRetInfos UpdateColValues(string Empid,string colName, string colValue, string agreementId, int roleType)
        {
            APDao_Agreement dao = new APDao_Agreement();
            return dao.UpdateColValues(Empid,colName, colValue, agreementId, roleType);
        }
        [WebMethod(Description = "销售反馈查询（勾选时间）")]
        public SortableBindingList<SaleFeed> GetSaleFeedBackInfos(string Emproleid, string Empname, SerializableDictionary<string, string> sqlkeydict, string time)
        {
            APDao_Agreement dao = new APDao_Agreement();
            return dao.GetSaleFeedBackInfos(Emproleid, Empname,sqlkeydict, time);
        }

        [WebMethod(Description = "销售反馈查询")]
        public SortableBindingList<SaleFeed> GetSaleFeedBackInfo(string Emproleid, string Empname,SerializableDictionary<string, string> sqlkeydict)
        {
            APDao_Agreement dao = new APDao_Agreement();
            return dao.GetSaleFeedBackInfo(Emproleid, Empname,sqlkeydict);
        }
        [WebMethod(Description = "获取员工信息")]
        public SortableBindingList<EmpInfos> GetEmpInfo(SerializableDictionary<string, string> sqlkeydict)
        {
            APDao_Agreement dao = new APDao_Agreement();
            return dao.GetEmpInfo(sqlkeydict);
        }


        }
}
