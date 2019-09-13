using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace MapsExchange
{
    public class PoeTradeProcessor
    {
        private readonly List<string> DefaultPostData = new List<string>();
        private readonly string url = "http://poe.trade/search";

        public PoeTradeProcessor()
        {
            DefaultPostData.Add("league=Bestiary");
            DefaultPostData.Add("type=Map");
            DefaultPostData.Add("base=");
            DefaultPostData.Add("name=");
            DefaultPostData.Add("dmg_min=");
            DefaultPostData.Add("dmg_max=");
            DefaultPostData.Add("aps_min=");
            DefaultPostData.Add("aps_max=");
            DefaultPostData.Add("crit_min=");
            DefaultPostData.Add("crit_max=");
            DefaultPostData.Add("dps_min=");
            DefaultPostData.Add("dps_max=");
            DefaultPostData.Add("edps_min=");
            DefaultPostData.Add("edps_max=");
            DefaultPostData.Add("pdps_min=");
            DefaultPostData.Add("pdps_max=");
            DefaultPostData.Add("armour_min=");
            DefaultPostData.Add("armour_max=");
            DefaultPostData.Add("evasion_min=");
            DefaultPostData.Add("evasion_max=");
            DefaultPostData.Add("shield_min=");
            DefaultPostData.Add("shield_max=");
            DefaultPostData.Add("block_min=");
            DefaultPostData.Add("block_max=");
            DefaultPostData.Add("sockets_min=");
            DefaultPostData.Add("sockets_max=");
            DefaultPostData.Add("link_min=");
            DefaultPostData.Add("link_max=");
            DefaultPostData.Add("sockets_r=");
            DefaultPostData.Add("sockets_g=");
            DefaultPostData.Add("sockets_b=");
            DefaultPostData.Add("sockets_w=");
            DefaultPostData.Add("linked_r=");
            DefaultPostData.Add("linked_g=");
            DefaultPostData.Add("linked_b=");
            DefaultPostData.Add("linked_w=");
            DefaultPostData.Add("rlevel_min=");
            DefaultPostData.Add("rlevel_max=");
            DefaultPostData.Add("rstr_min=");
            DefaultPostData.Add("rstr_max=");
            DefaultPostData.Add("rdex_min=");
            DefaultPostData.Add("rdex_max=");
            DefaultPostData.Add("rint_min=");
            DefaultPostData.Add("rint_max=");
            DefaultPostData.Add("mod_name=");
            DefaultPostData.Add("mod_min=");
            DefaultPostData.Add("mod_max=");
            DefaultPostData.Add("mod_weight=");
            DefaultPostData.Add("group_type=And");
            DefaultPostData.Add("group_min=");
            DefaultPostData.Add("group_max=");
            DefaultPostData.Add("group_count=1");
            DefaultPostData.Add("q_min=");
            DefaultPostData.Add("q_max=");
            DefaultPostData.Add("level_min=");
            DefaultPostData.Add("level_max=");
            DefaultPostData.Add("ilvl_min=");
            DefaultPostData.Add("ilvl_max=");
            DefaultPostData.Add("rarity=");
            DefaultPostData.Add("seller=");
            DefaultPostData.Add("thread=");
            DefaultPostData.Add("identified=");
            DefaultPostData.Add("corrupted=");
            DefaultPostData.Add("progress_min=");
            DefaultPostData.Add("progress_max=");
            DefaultPostData.Add("sockets_a_min=");
            DefaultPostData.Add("sockets_a_max=");
            DefaultPostData.Add("shaper=");
            DefaultPostData.Add("elder=");
            DefaultPostData.Add("online=x");
            DefaultPostData.Add("has_buyout=1");
            DefaultPostData.Add("altart=");
            DefaultPostData.Add("capquality=x");
            DefaultPostData.Add("buyout_min=");
            DefaultPostData.Add("buyout_max=");
            DefaultPostData.Add("buyout_currency=");
            DefaultPostData.Add("crafted=");
            DefaultPostData.Add("enchanted=");
        }

        public void OpenBuyMap(string mapName, bool isUniq, string league)
        {
            mapName = mapName.Replace("The ", "");
            var qParms = new List<string>(DefaultPostData);
            InsertChangeData(qParms, "name", mapName.Replace(" ", "+"));

            if (isUniq)
                InsertChangeData(qParms, "rarity", "unique");

            InsertChangeData(qParms, "league", league);

            using (var wb = new MyWebClient())
            {
                var queryString = ToQueryString(qParms);

                var response = wb.UploadString(url, "POST", queryString);

                var urlOpen = wb.ResponseUri.AbsoluteUri;
                Process.Start(urlOpen);
            }
        }

        private int InsertChangeData(List<string> data, string parm, string newData)
        {
            var pos = 0;

            for (var i = 0; i < data.Count; i++)
            {
                if (data[i].StartsWith(parm + "="))
                {
                    data[i] = parm + "=" + newData;
                    pos = i;
                }
            }

            return pos;
        }

        private string ToQueryString(List<string> nvc)
        {
            var array = nvc.ToArray();
            return string.Join("&", array);
        }

        private class MyWebClient : WebClient
        {
            public Uri ResponseUri { get; private set; }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                var response = base.GetWebResponse(request);
                ResponseUri = response.ResponseUri;
                return response;
            }
        }
    }
}
