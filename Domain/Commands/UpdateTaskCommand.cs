﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Commands
{
   public class UpdateTaskCommand
    {
        public Guid Id { get; set; }

        public Guid? AssignedToId { get; set; }
        public string Subject { get; set; }
        public bool IsComplete { get; set; }
    }
}