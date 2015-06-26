using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using WebCMD.Net;
using WebCMD.Core;
using WebCMD.Net.IO;
using WebCMD.Util;
using WebCMD.Com;
using WebCMD.Util.Html;

//TODO: Better error handeling 

namespace WebCMD.Lib.IssueDB
{
    public class CMD_Issue_Database : Command
    {
        public string issueDBPath = @"C:\Servers\Web\WebCMD.Issue.db";

        public CMD_Issue_Database()
        {
            Label = "Get-Issues";
            SetAliase("issues", "issue", "bugs", "bug", "todos", "todo");
        }

        protected override bool _Execute(CommandRequest e)
        {
            string[] args = e.ArgumentList;
            ServerResponse response = ResponseHandler.NewOutputResponse;

            try
            {
                // 0
                if (args.Length <= 0)
                {
                    List<Issue> l = GetIssueList();

                    foreach (Issue o in l)
                    {
                        if (o.State == Issue.IssueState.NEW || o.State == Issue.IssueState.WORK)
                            response.AddData(CreateHtmlEntry(o));
                    }
                }
                // 3
                //PRIORITY
                else if (check(args, 3, "prio", "priority", "-p"))
                {
                    Issue o = GetObject(Int32.Parse(args[1]));
                    if (o != null)
                    {
                        o.Priority = Issue.GetPriority(args[2].ToLower().Trim());

                        response.AddData(HTML.CreateCssClassOutputLine("blue-light", "Changed: "));
                        response.AddData(CreateHtmlEntry(UpdateObject(o)));
                    }
                    else response.AddData(CmdMessage.Get(CmdMessage.Type.Error, HTML.Encode("Error: ID Not found!")));
                }
                // 2
                //ADD
                else if (check(args, 2, "add", "report"))
                {
                    List<Issue> l = GetIssueList();

                    string desc = Regex.Replace(e.ArgumentString.Substring(args[0].Length), "\\s+", " ").Trim();
                    string highlight = "";
                    Issue.IssueType type = Issue.IssueType.TODO;

                    //prio
                    Issue.IssuePriority prio = Issue.GetPriority(desc.ToLower().Trim().Substring(0, 1));

                    if (prio != Issue.IssuePriority.NORMAL) desc = desc.Trim().Substring(1).Trim();

                    //heightlight (e.g. "[JS] ...")
                    if (desc.ToLower().StartsWith("[") && desc.ToLower().Contains("]"))
                    {
                        highlight = desc.Substring(0, desc.IndexOf("]") + 1).Trim();
                        desc = desc.Substring(highlight.Length).Trim();
                    }

                    //bug
                    string b = e.Command.ToLower();
                    if (b.Contains("bug")) type = Issue.IssueType.BUG;
                    if (desc.ToLower().StartsWith("bug:"))
                    {
                        type = Issue.IssueType.BUG;
                        desc = desc.Substring(4).Trim();
                    }
                    //add
                    if (desc.ToLower().StartsWith("add:"))
                    {
                        type = Issue.IssueType.ADD;
                        desc = desc.Substring(4).Trim();
                    }

                    Issue o = new Issue(type, String.Concat(highlight, " ", desc));
                    o.ID = l[l.Count - 1].ID + 1;
                    o.State = Issue.IssueState.NEW;
                    o.Priority = prio;

                    l.Add(o);
                    SaveIssueList(l);
                    response.AddData(HTML.CreateCssClassOutputLine("green", "Added: "));
                    response.AddData(CreateHtmlEntry(o));
                }
                //REMOVE
                else if (check(args, 2, "remove", "-rm"))
                {
                    List<Issue> l = GetIssueList();
                    int n = Int32.Parse(args[1]);
                    bool match = false;

                    foreach (Issue o in l)
                    {
                        if (o.ID == n)
                        {
                            match = true;
                            l.Remove(o);
                            response.AddData(HTML.CreateCssClassOutputLine("red", "Removed: "));
                            response.AddData(CreateHtmlEntry(o));
                            break;
                        }
                    }

                    if (match) SaveIssueList(l);
                    else response.AddData(CmdMessage.Get(CmdMessage.Type.Error, HTML.Encode("Error: ID Not found!")));
                }
                //FIXED
                else if (check(args, 2, "fixed", "-f", "done"))
                {
                    Issue o = GetObject(Int32.Parse(args[1]));
                    if (o != null)
                    {
                        o.State = Issue.IssueState.DONE;
                        if (o.Type == Issue.IssueType.BUG) o.State = Issue.IssueState.FIXED;

                        response.AddData(HTML.CreateCssClassOutputLine("blue-light", "Changed: "));
                        response.AddData(CreateHtmlEntry(UpdateObject(o)));
                    }
                    else response.AddData(CmdMessage.Get(CmdMessage.Type.Error, HTML.Encode("Error: ID Not found!")));
                }
                //INVALID
                else if (check(args, 2, "invalid", "-i"))
                {
                    Issue o = GetObject(Int32.Parse(args[1]));
                    if (o != null)
                    {
                        o.State = Issue.IssueState.INVALID;
                        response.AddData(HTML.CreateCssClassOutputLine("blue-light", "Changed: "));
                        response.AddData(CreateHtmlEntry(UpdateObject(o)));
                    }
                    else response.AddData(CmdMessage.Get(CmdMessage.Type.Error, HTML.Encode("Error: ID Not found!")));
                }
                //NEW 
                else if (check(args, 2, "new", "-n"))
                {
                    Issue o = GetObject(Int32.Parse(args[1]));
                    if (o != null)
                    {
                        o.State = Issue.IssueState.NEW;
                        response.AddData(HTML.CreateCssClassOutputLine("blue-light", "Changed: "));
                        response.AddData(CreateHtmlEntry(UpdateObject(o)));
                    }
                    else response.AddData(CmdMessage.Get(CmdMessage.Type.Error, HTML.Encode("Error: ID Not found!")));
                }
                //IN PROGRESS
                else if (check(args, 2, "work", "assign", "-w", "-as"))
                {
                    Issue o = GetObject(Int32.Parse(args[1]));
                    if (o != null)
                    {
                        o.State = Issue.IssueState.WORK;
                        response.AddData(HTML.CreateCssClassOutputLine("blue-light", "Changed: "));
                        response.AddData(CreateHtmlEntry(UpdateObject(o)));
                    }
                    else response.AddData(CmdMessage.Get(CmdMessage.Type.Error, HTML.Encode("Error: ID Not found!")));
                }
                // 1
                //LIST
                else if (check(args, 1, "list"))
                {
                    List<Issue> l = GetIssueList();

                    List<Issue.IssueState> filters = new List<Issue.IssueState>();

                    if (args.Length >= 2)
                    {
                        for (int i = 1; i < args.Length; i++)
                        {
                            try
                            {
                                filters.Add((Issue.IssueState)Enum.Parse(typeof(Issue.IssueState), args[i], true));
                            }
                            catch { filters.Clear(); }
                        }
                    }

                    foreach (Issue o in l)
                    {
                        if (filters.Count <= 0) response.AddData(CreateHtmlEntry(o));
                        foreach (Issue.IssueState filter in filters)
                        {
                            if (o.State == filter) response.AddData(CreateHtmlEntry(o));
                        }
                    }
                }
                else
                {
                    PrintUsage(response);
                }
            }
            catch(Exception ex2)
            {
                PrintUsage(response);
                response.AddData("ERROR: ", ex2.ToString());
            }

            e.Source.Response.Send(response);

            return true;
        }

        public void PrintUsage(ServerResponse response)
        {
            response.AddData(HTML.CreateOutputLine("<br/><br/>", Color.Get("orange", "WebCMD Issue DB"), " -- Version 2.2 -- Team Icelane (c) 2015 <br/><br/>"));
            
            response.AddData(HTML.CreateOutputLine("Command to manage the WebCMD issue database.<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(HTML.Encode("  Aliases: "), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(HTML.Encode("    "), Color.Get("orange", HTML.Encode(String.Join(", ", this.Aliase))), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(HTML.Encode("  Usage:"), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(HTML.Encode("    "), Color.Get("orange", "issues"), " [list|add|remove|fixed|invalid|new|work|priority] <br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      list [state]"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Get a list of issues based on the given issue-state."), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Available issue-states:")));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("green-light", Issue.IssueState.FIXED.ToString())));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("green-light", Issue.IssueState.DONE.ToString())));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("blue-light", Issue.IssueState.NEW.ToString())));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("red", Issue.IssueState.INVALID.ToString())));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("orange", Issue.IssueState.WORK.ToString()), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      add|report <description>"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Add a issue/todo or report a bug with the given description text."), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Use the command alias "), Color.Get("orange", "\"bug\""), HTML.Encode(" to report the issue or add "), Color.Get("orange", HTML.Encode("\"Bug:\""))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          at the beginning of the description text to mark the issue as a bug.")));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Add "), Color.Get("orange", HTML.Encode("\"Add:\"")), HTML.Encode(" at the beginning of the description text to mark the issue as an addition."), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Text within square brackets ("), Color.Get("blue-light", HTML.Encode("[]")), HTML.Encode(") at the beginning of the description text")));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          will be highlighted with a color."), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      remove|-rm <id>"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Remove the issue with the given id."), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      fixed|-f <id>"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Mark the issue with the given id as "), Color.Get("green-light", Issue.IssueState.FIXED.ToString()), " / ", Color.Get("green-light", "DONE"), ".", "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      invalid|-i <id>"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Mark the the issue with the given id as an "), Color.Get("red", Issue.IssueState.INVALID.ToString()), " issue.", "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      new|-n <id>"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Mark the issue with the given id as a "), Color.Get("blue-light", Issue.IssueState.NEW.ToString()), " issue.", "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      work|assig|-w|-as <id>"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Mark the issue with the given id as "), Color.Get("orange", Issue.IssueState.WORK.ToString()), ", wich means someone is working on it.", "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(Color.Get("orange",     HTML.Encode("      priority|prio|-p <id> <priority>"))));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Set the issue priority."), "<br/><br/>"));

            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("          Available priority:")));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("red", "!"), " (critical)"));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("orange", "+"), " (heigh)"));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), ".", " (normal)"));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("blue-light", "-"), " (low)"));
            response.AddData(HTML.CreateOutputLine(                        HTML.Encode("           - "), Color.Get("green-light", "?"), " (suggestion)", "<br/><br/>"));
        }

        private bool check(string[] args, int minargs, params string[] commands)
        {
            if (args.Length < minargs) return false;
            string arg = args[0].Trim().ToLower();

            foreach (string com in commands)
            {
                if (com.Trim().ToLower().Equals(arg)) return true;

            }
            return false;
        }

        public string CreateHtmlEntry(Issue o)
        {
            string tmpla = "<div>" +
                           "<span class=\"yellow\" style=\"display: table-cell;\">{0}</span>" +
                           "<span class=\"{1}\" style=\"display: table-cell;\">{2}</span>" +
                           "<span class=\"{3}\" style=\"display: table-cell;\">{4}</span>" +
                           "<span style=\"display: table-cell;\"><span class=\"{5}\">{6}</span><span class=\"blue-light\">{7}</span>{8}</span>" +
                           "</div>";
            
            string id = HTML.Encode(String.Format("{0, -6}", o.ID.ToString("D4")));
            string state = HTML.Encode(String.Format("{0, -13}", o.State).Replace("_", " "));
            
            string statecolor = o.State == Issue.IssueState.INVALID ? "red" :
                                   o.State == Issue.IssueState.WORK ? "orange" :
                                   o.State == Issue.IssueState.FIXED || o.State == Issue.IssueState.DONE ? "green-light" : "blue-light";

            string prio = HTML.Encode(String.Format("{0, -5}", o.GetPriority()));
            string priocolor = o.Priority == Issue.IssuePriority.CRITICAL ? "red" :
                                o.Priority == Issue.IssuePriority.HEIGH ? "orange" :
                                o.Priority == Issue.IssuePriority.SUGGESTION ? "green-light" :
                                o.Priority == Issue.IssuePriority.QUESTION ? "yellow" :
                                o.Priority == Issue.IssuePriority.LOW ? "blue-light" : "gray-light";

            string typeColor = "white";
            string type = "";

            if (o.Type == Issue.IssueType.BUG)
            {
                type = HTML.Encode("Bug: ");
                typeColor = "red";
            }
            else if (o.Type == Issue.IssueType.ADD)
            {
                type = HTML.Encode("Add: ");
                typeColor = "green-light";
            }
            
            string highlight = "";
            string finalDesc = o.Description.Trim();

            if (finalDesc.ToLower().StartsWith("[") && finalDesc.ToLower().Contains("]"))
            {
                highlight = finalDesc.Substring(0, finalDesc.IndexOf("]") + 1).Trim();
                finalDesc = finalDesc.Substring(highlight.Length);
            }

            return String.Format(tmpla, id, statecolor, state, priocolor, prio, typeColor, type, HTML.Encode(highlight), HTML.Encode(finalDesc));
        }

        public Issue UpdateObject(Issue obj)
        {
            List<Issue> l = GetIssueList();
            Issue nObj = null;

            for (int index = 0; index < l.Count; index++)
			{
                nObj = l[index];
                if (nObj.ID == obj.ID)
                {
                    nObj.State = obj.State;
                    nObj.Priority = obj.Priority;
                    nObj.Description = obj.Description;
                    break;
                }
            }

            SaveIssueList(l);
            return nObj;
        }
        
        public Issue GetObject(int id)
        {
            List<Issue> l = GetIssueList();
            Issue nObj = null;

            for (int index = 0; index < l.Count; index++)
            {
                if (l[index].ID != id) continue;
                nObj = l[index];
                break;
            }

            return nObj;
        }

        private List<Issue> GetIssueList()
        {
            int counter = 0;
            string line;
            List<Issue> l = new List<Issue>();

            System.IO.StreamReader file = null;
            try
            {
                // Read the file and display it line by line.
                file = new System.IO.StreamReader(issueDBPath);
                while ((line = file.ReadLine()) != null)
                {
                    string[] tbl = line.Split('\t');

                    Issue.IssueType bt = (Issue.IssueType)Enum.Parse(typeof(Issue.IssueType), tbl[2].Trim(), true);

                    Issue o = new Issue(bt, tbl[4].Trim());
                    o.ID = Int32.Parse(tbl[0].Trim());
                    o.State = (Issue.IssueState) Enum.Parse(typeof(Issue.IssueState), tbl[1].Trim(), true);
                    o.Priority = Issue.GetPriority(tbl[3].Trim());

                    // convert
                    string desc = o.Description.Trim();
                    string highlight = "";

                    //heightlight (e.g. "[JS] ...")
                    if (desc.ToLower().StartsWith("[") && desc.ToLower().Contains("]"))
                    {
                        highlight = desc.Substring(0, desc.IndexOf("]") + 1).Trim();
                        desc = desc.Substring(highlight.Length).Trim();
                    }

                    if (o.Type == Issue.IssueType.TODO)
                    {
                        if (desc.ToLower().StartsWith("bug:"))
                        {
                            o.Type = Issue.IssueType.BUG;
                            o.Description = String.Concat(highlight, " ", desc.Substring(4).Trim()).Trim();
                        }
                        else if (desc.ToLower().StartsWith("add "))
                        {
                            o.Type = Issue.IssueType.ADD;
                        }
                        else if (desc.ToLower().StartsWith("add:"))
                        {
                            o.Type = Issue.IssueType.ADD;
                            o.Description = String.Concat(highlight, " ", desc.Substring(4).Trim()).Trim();
                        }
                    }
                    
                    
                    l.Add(o);

                    counter++;
                }

                file.Close();
            }
            catch (Exception)
            {
                if (file != null) file.Close();
            }

            return l;
        }

        private void SaveIssueList(List<Issue> l)
        {
            // Read the file and display it line by line.
            System.IO.StreamWriter file = new System.IO.StreamWriter(issueDBPath);

            foreach (Issue o in l)
            {
                string s = o.ToString();
                file.WriteLine(s);
            }

            file.Close();
        }

    }

    public class Issue
    {
        public enum IssueState{
            FIXED, DONE, NEW, INVALID, WORK
        }

        public enum IssueType
        {
            TODO, BUG, ADD
        }

        public enum IssuePriority
        {
            CRITICAL, HEIGH, NORMAL, LOW, SUGGESTION, QUESTION
        }

        public int ID { get; set; }
        public String Description { get; set; }
        public IssueState State { get; set; }
        public IssueType Type { get; set; }
        public IssuePriority Priority { get; set; }

        public Issue(IssueType t, string txt)
        {
            Type = t;
            State = IssueState.NEW;
            Description = txt;
            Priority = IssuePriority.NORMAL;
            ID = 0;
        }

        public override string ToString()
        {
            return String.Format("{0}\t{1}\t{2}\t{3}\t{4}", ID, State, Type, Priority, Description);
        }

        public string GetPriority()
        {
            switch (Priority)
            {
                case IssuePriority.CRITICAL:
                    return "[!]";
                case IssuePriority.HEIGH:
                    return "[+]";
                case IssuePriority.NORMAL:
                    return "[ ]";
                case IssuePriority.LOW:
                    return "[-]";
                case IssuePriority.SUGGESTION:
                    return "[?]";
                case IssuePriority.QUESTION:
                    return "[?]";
                default:
                    return "[ ]";
            }
        }

        public static IssuePriority GetPriority(string p)
        {
            IssuePriority e = IssuePriority.NORMAL;
            try { e = (IssuePriority)Enum.Parse(typeof(IssuePriority), p, true); }
            catch
            {
                switch (p.Replace("[", "").Replace("]", ""))
                {
                    case "!":
                        e = IssuePriority.CRITICAL;
                        break;
                    case "+":
                        e = IssuePriority.HEIGH;
                        break;
                    case "":
                        e = IssuePriority.NORMAL;
                        break;
                    case "-":
                        e = IssuePriority.LOW;
                        break;
                    case "?":
                        e = IssuePriority.SUGGESTION;
                        break;
                    default:

                        IssuePriority[] bps = (IssuePriority[])Enum.GetValues(typeof(IssuePriority));

                        foreach (IssuePriority bp in bps)
                        {
                            if (p.Length < 3) break;
                            if (bp.ToString().ToLower().StartsWith(p))
                                e = bp;
                        }

                        break;
                }   
            }
            return e;
        }
    }

}