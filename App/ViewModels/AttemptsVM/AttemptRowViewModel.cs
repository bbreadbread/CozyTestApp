using Safety_Wheel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.ViewModels.AttemptsVM
{
    public class AttemptRowViewModel
    {
        public Attempt Attempt { get; }
        public DateTime? Started => Attempt.StartedAt;
        public int? Score => Attempt.Score;

        public AttemptRowViewModel(Attempt attempt)
        {
            Attempt = attempt;
        }
    }

}
