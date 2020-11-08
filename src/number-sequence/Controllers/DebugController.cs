//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Caching.Memory;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;

//namespace number_sequence.Controllers
//{
//    [ApiController, Route("[controller]")]
//    public sealed class DebugController : ControllerBase
//    {
//        private readonly IMemoryCache memoryCache;

//        public DebugController(IMemoryCache memoryCache)
//        {
//            this.memoryCache = memoryCache;
//        }

//        [HttpGet("dumpcache")]
//        public IActionResult DumpCache()
//        {
//            FieldInfo field = typeof(MemoryCache).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
//            var obj = (IDictionary)field.GetValue(this.memoryCache);

//            var values = new List<object>();
//            foreach (var value in obj.Values)
//            {
//                var type = value.GetType();
//                values.Add(new
//                {
//                    AbsoluteExpiration = type.GetProperty("AbsoluteExpiration")?.GetValue(value),
//                    AbsoluteExpirationRelativeToNow = type.GetProperty("AbsoluteExpirationRelativeToNow")?.GetValue(value),
//                    SlidingExpirationTotalMinutes = ((TimeSpan?)type.GetProperty("SlidingExpiration")?.GetValue(value))?.TotalMinutes,
//                    Priority = type.GetProperty("Priority")?.GetValue(value),
//                    Size = type.GetProperty("Size")?.GetValue(value),
//                    Key = type.GetProperty("Key")?.GetValue(value),
//                    Value = type.GetProperty("Value")?.GetValue(value)?.ToString(),
//                    LastAccessed = type.GetProperty("LastAccessed")?.GetValue(value),
//                    EvictionReason = type.GetProperty("EvictionReason")?.GetValue(value)
//                });
//            }

//            return this.Ok(values);
//        }
//    }
//}
