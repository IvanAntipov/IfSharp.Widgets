[<AutoOpen>]
module IfSharpWidgets =
    open IfSharp.Kernel
    open IfSharp.Kernel.Globals
    open System.Text
    open System.Web
    open System

    let clickHandlers = new System.Collections.Generic.Dictionary<string, (unit->unit)>()
    let executeClickHandler (elementId: string) = clickHandlers.[elementId]()
    let printHtml html = 
        App.Kernel.Value.SendDisplayData("text/html", html)
    let internal newId() =  System.Guid.NewGuid().ToString()
        
    let renderCheckBoxHtml title isChecked elementId =
        let encodedTitle = title |> System.Web.HttpUtility.HtmlEncode
        let html = sprintf """<input onclick="ifWidgetsClick(&quot;IfSharpWidgets.executeClickHandler \&quot;%s\&quot;&quot;) " type="checkbox" %s>%s</button>""" elementId (if isChecked then "checked" else "") encodedTitle
        html
    let createCheckBox title isChecked onClick =
        let elementId = newId()
        clickHandlers.[elementId] <- onClick
        renderCheckBoxHtml title isChecked elementId |> Util.Html
    type CheckBoxModel(title, getValue, elementId) =
        member this.RenderHtml() = renderCheckBoxHtml title (getValue()) elementId
        member this.Value with get() = (getValue())
        member this.Display() = printHtml (this.RenderHtml())
        
    let CheckBox(title, isChecked) =
        let state = ref isChecked
        let elementId = newId()
        let onClick() = state:= (!state |> not)
        clickHandlers.[elementId] <- onClick
        let getState() = !state
        CheckBoxModel(title, getState , elementId)

    type IfTableCell = | Text of string | Html of string
    type IfTableOutput = 
        {
            Columns: array<string>;
            Rows: array<array<IfTableCell>>;
        }
        member this.ToHtml() =
            let htmlEncode str = HttpUtility.HtmlEncode(str)
            let sb = StringBuilder()
            sb.Append("<table>") |> ignore

            // output header
            sb.Append("<thead>") |> ignore
            sb.Append("<tr>") |> ignore
            for col in this.Columns do
                sb.Append("<th>") |> ignore

                sb.Append(htmlEncode col) |> ignore
                sb.Append("</th>") |> ignore
            sb.Append("</tr>") |> ignore
            sb.Append("</thead>") |> ignore

            // output body
            sb.Append("<tbody>") |> ignore
            for row in this.Rows do
                sb.Append("<tr>") |> ignore
                for cell in row do
                    sb.Append("<td>") |> ignore
                    let cellContent = match cell with | Text x -> htmlEncode x | Html x -> x
                    sb.Append(cellContent) |> ignore
                    sb.Append("</td>") |> ignore

                sb.Append("</tr>") |> ignore
            sb.Append("<tbody>") |> ignore
            sb.Append("</tbody>") |> ignore
            sb.Append("</table>") |> ignore
            sb.ToString()

    type Util =
        static member Row (columns:seq<Reflection.PropertyInfo>) (item:'A) =
            columns
            |> Seq.map (fun p -> p.GetValue(item))
            |> Seq.map (fun x ->
                    match x with
                    | :? CheckBoxModel as m -> Html (m.RenderHtml())
                    | o -> Text (Convert.ToString(o)))
            |> Seq.toArray  
        static member  Table (items:seq<'A>, ?propertyNames:seq<string>)   =
            // get the properties
            let properties =
                if propertyNames.IsSome then
                    typeof<'A>.GetProperties()
                    |> Seq.filter (fun x -> (propertyNames.Value |> Seq.exists (fun y -> x.Name = y)))
                    |> Seq.toArray
                else
                    typeof<'A>.GetProperties()

            {
                IfTableOutput.Columns = properties |> Array.map (fun x -> x.Name);
                Rows = items |> Seq.map (Util.Row properties) |> Seq.toArray;
            }          
        
    let initialize() =
        let shellMessageScript = """function ifWidgetsClick(objectId){
callbacks = {};
var content = {
    code : objectId,
    silent : true,
    store_history : false,
    user_expressions : {},
    allow_stdin : false
};
IPython.notebook.kernel.send_shell_message("execute_request", content, callbacks);
}"""
        let addScript (script: string) =
            let escapeJson (str: string) = str.Replace("\"","\\\"").Replace("\r","").Replace("\n","").Replace("\t","")
            let scriptToAdd =
                sprintf """var ifWidgetsScript = document.createElement("script");
ifWidgetsScript.type = "text/javascript";
var ifWidgetsScriptBody = document.createTextNode("%s");
ifWidgetsScript.appendChild(ifWidgetsScriptBody);
document.getElementsByTagName("head")[0].appendChild(ifWidgetsScript);
"""                    (escapeJson script)   
            
            App.Kernel.Value.SendDisplayData("application/javascript", scriptToAdd)
        addScript shellMessageScript  
        let addIfTablePrinter() =
            
        // add table printer
            Printers.addDisplayPrinter(fun (x:IfTableOutput) ->
                let html = x.ToHtml()

                { ContentType = "text/html"; Data = html }
            )  
        addIfTablePrinter()        
        App.AddDisplayPrinter(fun (x:CheckBoxModel) -> {ContentType = "text/html"; Data = (x.RenderHtml())})
        App.AddFsiPrinter(fun (i: CheckBoxModel) -> sprintf "CheckBox Value=%b" i.Value)
IfSharpWidgets.initialize()
