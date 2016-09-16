using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode.TypeWriter
{
    public class Dummy : TypeWriter
    {
        public override void writeType(TypeData data)
        {
            /** Type Signature **/

            /// Name ///
            writer.Write(" ");
            writer.Write(data.Name);
            OpenBlock();

            /** Members **/
            //foreach(var field in data.getFields()) writeField(field);
            //foreach(var prop in data.getProperties()) writeProperty(prop);

            writer.WriteLine("/* Fields */");
            writer.WriteLine("/* Prop */");
            writer.WriteLine("/* Ctor */");
            writer.WriteLine("/* Cctor */");
            writer.WriteLine("/* Dtor */");

            writeMethods(data.getMethods());
            // Nested Types
            CloseBlock();
        }

        // <Scope> <static|const|....> TYPE NAME <;| = VALUE;>
        //public override void writeField(FieldData data) {}
        //public override void writeProperty(PropData data) {}

        // Ofc the user is free to just collect the build code in seperate lists in the class
        // and assemble them in the correct order after the loop
        public override void writeMethods(List<MethodData> dataList)
        {
            NewLine();
            foreach (var meth in dataList) {
                writer.WriteLine(meth.ToString(this));
            }
        }

        public override void writeCtor(MethodData data) { }
        public override void writeCctor(MethodData data) { }
        public override void writeDtor(MethodData data) { }
        public override void writeMethod(MethodData data, bool isEntry)
        {
            if (!isEntry) {
                // write Signature
                writer.Write(" " + data.Name);
                // Arguments
            } else writer.Write(data.getMainSignature());
            OpenBlock();
            WriteBlock(data.getLocalOutput());
            NewLine();
            WriteBlock(data.Code);
            CloseBlock();
        }
    }
}