///
/// Scala-like XML parsing for F#.
///

open System
open System.Xml
open System.IO
open System.Xml.XPath

// Select nodes.
let inline (+/) (nav: XPathNavigator) (path: string) =
  let iter = nav.Select(path)
  seq { while iter.MoveNext() do yield iter.Current }

// Select a single node.
let inline (+//) (nav: XPathNavigator) (path: string) =
  nav.SelectSingleNode(path)

// Get the value of a node.
let inline (+//>) (nav: XPathNavigator) (path: string) =
  nav.SelectSingleNode(path).Value

// Get the value of specified attribute for the current node.
let inline (+//>>)  (nav: XPathNavigator) (attr: string) =
  nav.GetAttribute(attr, String.Empty)

// Some dummy content.
let content = "\
<AddressBook>
  <Owner firstName=\"John\" lastName=\"Smith\" />
  <Contacts>
    <Contact firstName=\"Jane\" lastName=\"Smith\">
      <Email>jane@smith.com</Email>
      <Phone type=\"mobile\">617-555-3311</Phone>
    </Contact>
    <Contact firstName=\"Bob\" lastName=\"Notsmith\">
      <Email>bob@gmail.com</Email>
      <Phone type=\"office\">716-111-2222</Phone>
    </Contact>
  </Contacts>
</AddressBook>"

// Something that we can parse into.
type Contact = {
  Email : string
  Phone : string
  PhoneType : string
}

[<EntryPoint>]
let main args = 
  use stream = new StringReader(content)
  let nav = XPathDocument(stream).CreateNavigator()
  let firstName = nav +// "/AddressBook/Owner" +//>> "firstName"
  let lastName = nav +// "/AddressBook/Owner" +//>> "lastName"
  
  let contacts =
    nav +/ "/AddressBook/Contacts/Contact"
    |> Seq.map (fun c -> 
                 ({ Email = c +//> "Email";
                    Phone = c +//> "Phone";
                    PhoneType = c +// "Phone" +//>> "type" }))
    |> List.ofSeq

  0