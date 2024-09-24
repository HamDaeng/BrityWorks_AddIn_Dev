using BrityWorks.AddIn.Hyper.Properties;
using RPAGO.AddIn;
using RPAGO.Common.Data;
using RPAGO.Common.Library;
using System;
using System.Collections.Generic;
using Bitmap = System.Drawing.Bitmap;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript;
using Win32;

namespace BrityWorks.AddIn.Hyper.Activities
{
    public class CloseWindowHwnd : IActivityItem
    {
        public static readonly PropKey OutputPropKey = new PropKey("Close", "Prop1");

        public static readonly PropKey InputPropKey = new PropKey("Close", "Prop2");

        public static readonly PropKey ForcePropKey = new PropKey("Close", "Prop3");

        public static readonly PropKey OnOffPropKey = new PropKey("Close", "Prop4");

        public string DisplayName => "Close Window Hwnd";

        public Bitmap Icon => Resources.excute;

        public LibraryHeadlessType Mode => LibraryHeadlessType.Both;

        public PropKey DisplayTextProperty => OutputPropKey;

        public PropKey OutputProperty => OutputPropKey;

        private PropertySet PropertyList;

        public List<Property> OnCreateProperties()
        {
            var properties = new List<Property>()
            {
                new Property(this, OutputPropKey, "RESULT").SetRequired(),
                new Property(this, InputPropKey, "").SetRequired(),

                new Property(this, ForcePropKey, false).SetControlType(PropertyControls.PropertyItemToggleView),
                new Property(this, OnOffPropKey, false).SetControlType(PropertyControls.PropertyItemToggleView)
            };

            return properties;
        }

        public void OnLoad(PropertySet properties)
        {
            PropertyList = properties;
        }

        // 창 닫기 전용
        public static void close_window(IntPtr hwnd, bool force, int outs)
        {
            // 혹시 모르니, 한번 Active
            User.SendMessageTimeout(hwnd, 0x1C, 1, 1, 0x0, 1, ref outs);

            // 강제종료 여부 확인
            if ( force )
            {
                // 강제종료시 WM_DESTRORY
                User.SendMessageTimeout(hwnd, 0x2, 1, 1, 0x0, 1000, ref outs);
            }
            else
            {
                // 일반 종료시 WM_CLOSE
                User.SendMessageTimeout(hwnd, 0x10, 1, 1, 0x0, 1000, ref outs);
            }

        }

        public object OnRun(IDictionary<string, object> properties)
        {
            // 클리어 스크립트 선언 ( 형변환 전용 )
            V8ScriptEngine v8 = new V8ScriptEngine();

            bool success = true;

            try
            {
                // properties[InputPorpKey] 자체가 object 형태이기 때문에, 전체가 아닌 해당 대상만 가져옴, hwnd 입력상태라면 hwnd을 가져옴
                var obj = properties[InputPropKey];

                // Hwmd 추출
                v8.AddHostObject("obj", HostItemFlags.GlobalMembers, obj);

                // hwnd 입력상태가 아니라면
                if ( (bool)properties[OnOffPropKey] == false )
                {
                    // Hwmd 추출
                    v8.Execute("var hwnd = obj.AppHwnd");
                }
                else
                {
                    // Hwmd 추출
                    v8.Execute("var hwnd = obj");
                }

                // 형변환 int
                var str = Convert.ToInt32(v8.Evaluate("hwnd"));

                // str 체크 ( 잘못된 대상이면 0 )
                if (str != 0)
                {
                    // 형변환 intptr
                    IntPtr cast_str = new IntPtr(str);

                    // 강제종료 여부
                    bool force_chk = (bool)properties[ForcePropKey];

                    // 창닫기 요청
                    close_window(cast_str, force_chk, 0);
                }
                // 뭐 이상한거 있었으면 오류 발생
                else
                {
                    success = false;
                }

            }
            // 오류 발생시 success = false, throw 발생
            catch(Exception e)
            {
                success = false;
                throw e.InnerException;
            }

            return success;
        }
    }
}
