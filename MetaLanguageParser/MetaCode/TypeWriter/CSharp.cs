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
			//var scope = new[]{"public", "protected", "internal", "private"};
			var scope = new[]{"Public", "Family", "Assembly", "FamANDAsm", "Private"};
			var access = new[]{"static"};
			var state = writeModifiers(data, scope, " ");
			state = writeModifiers(data, access, " ", state);
			writeMode(data, state);
			/// Name ///
			writer.Write(' ');
			writer.Write(data.Name);
			// Generics
			bool _ext = data.hasExt();
			bool _impl = data.hasImpl();
			if(_ext || _impl){
				writer.Write(" : ");
				if(_ext) writer.Write(data.getExt());
				if(_impl && _ext) writer.Write(", ");
				if(_impl) writeList(data.getInterfaces(), ",");
			}
			
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
		
		void __Meth1(TypeData data, bool _ext, bool _impl){
				writer.Write(" : ");
				if(_ext) writer.Write(data.getExt());
				if(_impl && _ext) writer.Write(", ");
				if(_impl) writeList(data.getInterfaces(), ",");
		}
		void __Meth2(TypeData data, bool _ext, bool _impl){
			if(_ext || _impl){
				writer.Write(" : ");
				if(_ext) writer.Write(data.getExt());
				if(_impl){
					if(_ext) writer.Write(", ");
					writeList(data.getInterfaces(), ", ", false);
				}
			}
		}
		void __Meth3(TypeData data, bool _ext, bool _impl){
			if(_ext || _impl){
				writer.Write(" : ");
				if(_ext) writer.Write(data.getExt());
				if(_impl){
					writeList(data.getInterfaces(), ", ", _ext);
				}
			}
		}
		void __Meth4(TypeData data, bool _ext, bool _impl){
			if(_ext || _impl){
				writer.Write(" : ");
				if(_ext) writer.Write(data.getExt());
				if(_impl) writeList(data.getInterfaces(), ", ", _ext);
			}
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