## CodeFiles ##
These Files will be non-public regardless of anythings
* ExeBuilder
  * Holds .... stuff.
* Parser
  * Main Parser logic for iterating tokens
* Tokenizer
  * Tokenizes all kinds of files
* Adapter
  * Contains adapter methods, to prevent exposing everything to the normal user

## DataFiles ##
* MetaType
  * Contains Information to store an Type (like Name, Arrayness, etc.)
* MetaData
  * Abstract base class for Members (Access, Modifiers, Name)
* TypeData
  * Contains Informations to store a classFile (like if Class/Interface/Struct, Fields, Methods, etc.)
* MethodData
  * Contains Informations to store a Method (Arguments, RetType, Code)
* LocalData
  * Contains Information to store a Local Variable (Type, Name, evtl. Value)