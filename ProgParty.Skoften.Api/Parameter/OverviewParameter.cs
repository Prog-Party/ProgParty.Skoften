using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgParty.Skoften.Api.Parameter
{
    public class OverviewParameter
    {
        public string Category { get; set; }

        public int Paging { get; set; }

        public bool StartOver { get; set; } = false;

        public OverviewType Type { get; set; } = OverviewType.PicGif;
    }

    public enum OverviewType
    {
        PicGif,
        EroDump
    }
}
