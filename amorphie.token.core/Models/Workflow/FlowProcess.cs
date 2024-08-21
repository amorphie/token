using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Enums;

namespace amorphie.token.core.Models.Workflow
{
    public class FlowProcess
    {
        public string Id { get; set; }
        public FlowStatus FlowStatus{get;set;}
        public dynamic? Result{get;set;}
        public int StatusCode{get;set;}
        public string? ErrorMessage{get;set;}
    }
}