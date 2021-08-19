﻿using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ToolFetchWordDetails : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnToList_Click(object sender, EventArgs e)
    {
        string txtWords = tbWords.Text;
        if (txtWords.Length == 0)
        {
            Response.Write("<script>alert('Please input word list!');</script>");
            return;
        }

        List<string> listWords = new List<string>();
        listWords = txtWords.Split(Environment.NewLine.ToCharArray()).ToList();
        lbWordList.Items.Clear();
        for (int i = 0; i<listWords.Count; i++)
        {
            if (listWords[i].Length >0)
            {
                ListItem li = new ListItem();
                li.Text = listWords[i];
                li.Value = i.ToString();
                lbWordList.Items.Add(li);
            }
          }
        Panel1.Visible = false;
        Panel2.Visible = true;
        Panel3.Visible = true;
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        Panel1.Visible = true;
        Panel2.Visible = false;
        Panel3.Visible = false;
    }

    protected void btnFetch_Click(object sender, EventArgs e)
    {
        string word = lbWordList.SelectedItem.Text;
        string url = "https://api.dictionaryapi.dev/api/v2/entries/en/"+ word;
        //string url = "https://dictionaryapi.dev/";
        string temp = MyHttpTool.HttpGet(url);
        txtWordBody.Text = temp.Substring(1, temp.Length - 2);
    }

    protected void btnInsertDB_Click(object sender, EventArgs e)
    {
        api.dictionaryapi.dev.Root myRoot;
        try
        {
            myRoot = JsonConvert.DeserializeObject<api.dictionaryapi.dev.Root>(txtWordBody.Text);
            lblParseResult.Text = myRoot.word;
        }
        catch (Exception ex)
        {
            lblParseResult.Text = ex.Message;
            return;
        }
           

        string connectionStr;

        connectionStr = ConfigurationManager.ConnectionStrings["worddbConnectionString"].ConnectionString;
        MySqlConnection conn = new MySqlConnection(connectionStr);
        try
        {
            conn.Open();
            string sqlRowCount = "SELECT count(id) FROM word ";
            int iRowCount = 0;
            MySqlCommand cmdRowCount = new MySqlCommand(sqlRowCount, conn);
            MySqlDataReader rdr = cmdRowCount.ExecuteReader();
            while (rdr.Read())
            {
                iRowCount = Int32.Parse(rdr.IsDBNull(0) ? "0" : rdr[0].ToString());
            }
            rdr.Close();
            iRowCount++;

            //Id, UnitId, ScheduleDate, ScheduleTitle, TimeUsed, Result
            string sql = "INSERT INTO Word (Id, UnitId, WordBody, Desc1, Desc2, Desc3, Desc4, Desc5, Desc6)" +
                                 "VALUES (@Id, @UnitId, @WordBody, @Desc1, @Desc2, @Desc3, @Desc4, @Desc5,@Desc6)";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("Id", (iRowCount.ToString()));
            cmd.Parameters.AddWithValue("UnitId", "14");
            cmd.Parameters.AddWithValue("WordBody", myRoot.word);
            int DescIndex = 0;
            foreach (api.dictionaryapi.dev.Meaning m in myRoot.meanings)
            {
                foreach (api.dictionaryapi.dev.Definition d in m.definitions)
                {
                    DescIndex++;
                    if (DescIndex <= 6)
                    {
                        string partOfSpeech = m.partOfSpeech;
                        string tempDesc = d.definition;
                        cmd.Parameters.AddWithValue("Desc" + DescIndex.ToString(), partOfSpeech + " " + tempDesc);
                    }
                }
            }

            for (DescIndex++; DescIndex <= 6; DescIndex++)
            {
                    cmd.Parameters.AddWithValue("Desc" + DescIndex.ToString(), "");
            }

            cmd.ExecuteNonQuery();
            //conn.Close();
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.ToString());
        }
        finally
        {
            conn.Close();
        }
    }

    protected void lbWordList_SelectedIndexChanged(object sender, EventArgs e)
    {
        lblWordTitle.Text = lbWordList.SelectedItem.Text;
        txtWordBody.Text = "";
    }
}



public class MyHttpTool
{
    /// <summary>
    /// Http同步Get同步请求
    /// </summary>
    /// <param name="url">Url地址</param>
    /// <param name="encode">编码(默认UTF8)</param>
    /// <returns></returns>
    public static string HttpGet(string url, Encoding encode = null)
    {
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        string result;

        try
        {
            var webClient = new WebClient { Encoding = Encoding.UTF8 };

            if (encode != null)
                webClient.Encoding = encode;

            result = webClient.DownloadString(url);
        }
        catch (Exception ex)
        {
            result = ex.Message;
        }

        return result;
    }
}

//使用https://json2csharp.com/生成下面的类，然后将他们放入namespace
namespace dictionaryapi.com//Merriam-Webster
{
    public class AppShortdef
    {
        public string hw { get; set; }
        public string fl { get; set; }
        public List<string> def { get; set; }
    }

    public class Target
    {
        public string tuuid { get; set; }
        public string tsrc { get; set; }
    }

    public class Meta
    {
        public string id { get; set; }
        public string uuid { get; set; }
        public string src { get; set; }
        public string section { get; set; }
        public string highlight { get; set; }
        public List<string> stems { get; set; }

        [JsonProperty("app-shortdef")]
        public AppShortdef AppShortdef { get; set; }
        public bool offensive { get; set; }
        public Target target { get; set; }
    }

    public class Sound
    {
        public string audio { get; set; }
    }

    public class Pr
    {
        public string ipa { get; set; }
        public Sound sound { get; set; }
    }

    public class Altpr
    {
        public string ipa { get; set; }
    }

    public class Hwi
    {
        public string hw { get; set; }
        public List<Pr> prs { get; set; }
        public List<Altpr> altprs { get; set; }
    }

    public class In
    {
        public string il { get; set; }
        public string @if { get; set; }
        public string ifc { get; set; }
    }

    public class Def
    {
        public List<List<List<object>>> sseq { get; set; }
    }

    public class Vr
    {
        public string vl { get; set; }
        public string va { get; set; }
    }

    public class Dro
    {
        public string drp { get; set; }
        public List<Def> def { get; set; }
        public List<Vr> vrs { get; set; }
    }

    public class Uro
    {
        public string ure { get; set; }
        public List<Pr> prs { get; set; }
        public string fl { get; set; }
        public List<List<object>> utxt { get; set; }
        public List<In> ins { get; set; }
        public string gram { get; set; }
    }

    public class Root
    {
        public Meta meta { get; set; }
        public int hom { get; set; }
        public Hwi hwi { get; set; }
        public string fl { get; set; }
        public List<In> ins { get; set; }
        public string gram { get; set; }
        public List<Def> def { get; set; }
        public List<Dro> dros { get; set; }
        public List<string> dxnls { get; set; }
        public List<string> shortdef { get; set; }
        public List<Uro> uros { get; set; }
    }
}

namespace api.dictionaryapi.dev
{
    public class Phonetic
    {
        public string text { get; set; }
        public string audio { get; set; }
    }

    public class Definition
    {
        public string definition { get; set; }
        public string example { get; set; }
        public List<string> synonyms { get; set; }
    }

    public class Meaning
    {
        private string _partOfSpeech = "";
        public string partOfSpeech
        {
            get { return _partOfSpeech; }
            set
            {
                if (value.Contains("exclamation"))
                { _partOfSpeech = "e."; }
                else if (value.Contains("noun"))
                { _partOfSpeech = "n."; }
                else if (value.Contains("verb"))
                { _partOfSpeech = "v."; }
                else if (value.Contains("adjective"))
                { _partOfSpeech = "adj."; }
                //else
                //{ _partOfSpeech = String.IsNullOrEmpty(value) ? "" : value; }
            }
        }
        public List<Definition> definitions { get; set; }
    }

    public class Root
    {
        public string word { get; set; }
        public List<Phonetic> phonetics { get; set; }
        public List<Meaning> meanings { get; set; }
    }
}