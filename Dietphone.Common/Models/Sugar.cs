﻿using System;

namespace Dietphone.Models
{
    public class Sugar : EntityWithId
    {
        public DateTime DateTime { get; set; }
        public float BloodSugar { get; set; }
    }
}