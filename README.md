# IfSharp.Widgets
Interactive widgets for IfSharp https://github.com/fsprojects/IfSharp

# Example
https://nbviewer.jupyter.org/github/IvanAntipov/IfSharp.Widgets/blob/master/examples/IfSharp.Widgets.Example.ipynb

#Nuget
IfSharp.Widgets

#Usage
```
#load "Paket.fsx"
Paket.Package [ "IfSharp.Widgets" ]
#load "packages\IfSharp.Widgets\IfSharpWidgets.fsx"
open IfSharpWidgets
let users = ["Ivan";"Boris";"Moi"]
type UserMode = {Name:string; IsSelected: CheckBoxModel}

let userModels = users |> List.map(fun name -> {Name=name; IsSelected= CheckBox("Select", true)})

userModels |> Util.Table

userModels 
    |> Seq.filter(fun i -> i.IsSelected.Value)
    |> Seq.iter(fun i -> printfn "User %s is selected" i.Name)
```

# Current status
CheckBox widget implemented

#Road map
1. Display interaction failures
2. Implement TextBox widget
