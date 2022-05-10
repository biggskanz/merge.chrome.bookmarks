﻿/// functions to convert bookmark collections to entry list or tree
module MergeBookmark.Convert

open Util
open Domain
open Parse

let htmlToBookmarkLine file =
    file
        |> Seq.map ParseHtmlLine
        |> Seq.choose id

// todo: cleanup/refactor
// should be a way to do this without mutating
let htmlToEntry file =
    let mutable index = 0
    let parents = ResizeArray<int>()
    do parents.Add(0)
    
    let folderToEntry folder =
        do index <- index + 1                   
        let result =
            Some (FolderEntry (
                index,
                parents.Item(parents.Count-1),
                folder))
        do parents.Add(index)
        result
        
    let markToEntry mark =
        do index <- index + 1
        Some (MarkEntry (
            index,
            parents.Item(parents.Count-1),
            mark))
        
    [for line in file do
        match (ParseHtmlLine line) with
        | Some (BookmarkLine.Folder folder) ->
            folderToEntry folder
        | Some (BookmarkLine.Mark mark) ->
            markToEntry mark            
        | Some BookmarkLine.ListClose ->
            do parents.RemoveAt(parents.Count-1)
            None    
        | _ -> None]
    |> List.choose id
    
let entryToMark (lst:Entry list) =
    lst
    |> List.map(fun x ->
        match x with
        | MarkEntry (_,_,mark) -> Some mark
        | _ -> None )
    |> List.choose id
          
// tree    
let entryToTree (list:Entry list) =
    let rec getChildren parentId = list |> List.choose (fun itm ->
        match itm with
        | MarkEntry (i,p,info) when p = parentId ->
            Some (LeafNode info)
        | FolderEntry (i,p,info) when p = parentId ->
            Some (InternalNode (info, getChildren i))
        | _ -> None )
        
    let root =
        list
        |> List.pick (fun itm ->
            match itm with
            | FolderEntry (i,p,info) when p = 0 ->
                Some (BookmarkTree.InternalNode (info, getChildren i))
            | _ -> failwith "no root node found")
    root
    
let markToTree (list:MarkInfo list) =
    list
    |> List.map (fun markinfo -> MarkEntry(-1,1,markinfo))
    |> List.append [FolderEntry(1,0,{name="import";date="";modified=""})]
    |> entryToTree
    
    
/// Convert bookmark file to List of MarkInfos
let HtmlToBookmark file =
    file
    |> htmlToEntry
    |> entryToMark
    
/// Convert bookmark file to BookmarkTree
let HtmlToTree file =
    file
    |> htmlToEntry
    |> entryToTree