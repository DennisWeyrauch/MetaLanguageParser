using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode.TypeWriter
{
    public class CSharp : TypeWriter
    {
		public override void writeType(TypeData data){
			// public, private, protected, internal
			// static
			// class interface struct enum
			/// Name ///
			writer.Write(" ");
			writer.Write(data.Name);
			// Generics
			OpenBlock();
			/*/foreach(var field in data.getFields()) writeField(field);/*/writer.Write("/* Fields */");//*/
			NewLine();
			/*/foreach(var prop in data.getProperties()) writeProperty(prop);/*/writer.Write("/* Prop */");//*/
			NewLine();
			/*/foreach(var ctor in data.getConstructors()) writeCtor(ctor);/*/writer.Write("/* Cctor */");//*/
			NewLine();
			/*/if(data.cctor != null) writeCctor(data.cctor);/*/writer.Write("/* Cctor */");//*/
			NewLine();
			/*/if(data.dtor != null) writeDtor(data.dtor);/*/writer.Write("/* Dtor */");//*/
			NewLine();
			writeMethods(data.getMethods());
			NewLine();
			// Nested Types
			CloseBlock();
		}
		
		// <Scope> <static|const|....> TYPE NAME <;| = VALUE;>
		//public override void writeField(FieldData data) {}
		//public override void writeProperty(PropData data) {}
		// Ofc the user is free to just collect the build code in seperate lists in the class
		// and assemble them in the correct order after the loop
		public override void writeMethods(List<MethodData> dataList) {
			NewLine();
			foreach(var meth in dataList){
				writer.WriteLine(meth.ToString(this));
				
			}
		}
		
		public override void writeCtor(MethodData data) {}
		public override void writeCctor(MethodData data) {}
		public override void writeDtor(MethodData data) {}
		public override void writeMethod(MethodData data, bool isEntry) {
			if(!isEntry) {
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