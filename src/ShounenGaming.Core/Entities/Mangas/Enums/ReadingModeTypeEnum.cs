using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas.Enums
{
    public enum ReadingModeTypeEnum
    {
        ALWAYS_VERTICAL, // Vertical
        ALWAYS_VERTICAL_PAGED, // Vertical but paged
        ALWAYS_HORIZONTAL, //Horizontal
        HORIZONTAL_MANGAS_OTHERS_VERTICAL //Horizontal for Mangas, Vertical for Manhuas & Manhwas
    }
}
