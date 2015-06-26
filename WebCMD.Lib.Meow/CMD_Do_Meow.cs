using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WebCMD.Com;
using WebCMD.Net;
using WebCMD.Net.IO;


namespace Meow
{
    public class CMD_Do_Meow : Command
    {
        public CMD_Do_Meow()
        {
            Label = "Do-Meow";
            SetAliase("meow", "nya", ":3", ";3");
        }

        protected override bool _Execute(CommandRequest e)
        {
            Client c = e.Source;
            c.Response.Send("MEOW ;3");
            return true;
        }
    }
}
