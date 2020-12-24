using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace colander_game.Models
{
    public class PaperModel
    {
        public string Words { get; set; }

        public string AuthorUserId { get; set; }
    }
}