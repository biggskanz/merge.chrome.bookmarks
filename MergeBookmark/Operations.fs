﻿module MergeBookmark.Operations

open System
open System.IO
open Util
    
let GetDiffMarksOfDirectory dir =
    let loadFile file =
        IO.ReadAllLines file
        |> Convert.HtmlToTree
        
    //let dir = Environment.CurrentDirectory    
    let files =
        DirectoryInfo(dir).GetFileSystemInfos("*.html")
        |> Array.sortByDescending(fun f -> f.LastWriteTime)
        |> Array.map (fun x -> x.FullName)
              
    let getDiffMarks = BookmarkTree.DiffMarks (loadFile files[0])                    
    
    let files = files |> Array.tail
    
    [for file in files do
        getDiffMarks (loadFile file)]
    |> List.concat
    |> List.distinct
    |> Convert.markToTree
    |> Build.DocumentFromTree
    |> Seq.toArray
    |> IO.WriteAllLines $"{(Time.ToUnixTimeSeconds DateTime.Now)}_test.html"
    
let GetUniqueMarksEntry dir =
    let loadFile file =
        IO.ReadAllLines file
        |> Convert.htmlToEntry
        
    //let dir = Environment.CurrentDirectory    
    let files =
        DirectoryInfo(dir).GetFileSystemInfos("*.html")
        |> Array.sortByDescending(fun f -> f.LastWriteTime)
        |> Array.map (fun x -> x.FullName)
        
    let marksAndParent = Entry.DiffMarksAndParent (loadFile files[0])
    
    let files = files |> Array.tail
    
    [for file in files do
        marksAndParent (loadFile file)]
    |> List.concat
    |> List.distinctBy fst
    
let GetUniqueMarksEntry_test file1 file2 =
    let loadFile file =
        IO.ReadAllLines file
        |> Convert.htmlToEntry
        
    let entries1 = loadFile file1
    let entries2 = loadFile file2
        
    let uniqueMarks =
        entries2
        |> Entry.DiffMarksAndParent entries1
        
    entries1
    |> Entry.InsertMarkAtParent uniqueMarks.Head
    |> List.sortBy (fun e -> e.id)