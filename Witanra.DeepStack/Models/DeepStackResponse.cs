using System;
using System.Collections.Generic;
using System.Text;

namespace Witanra.DeepStack.Models
{
    internal class DeepStackResponse
    {
        public bool success { get; set; }
        public DeepStackPredection[] predictions { get; set; }
    }
}