module RPN.PolishCalculator
// Mats RPN

type Stack = StackContents of float list
let EMPTY = StackContents []

// ==============================================
// Stack primitives
// ==============================================

/// Push a value on the stack
let push x (StackContents contents) =   
    StackContents (x::contents)

/// Pop a value from the stack and return it 
/// and the new stack as a tuple
let pop (StackContents contents) = 
    match contents with 
    | top::rest -> 
        let newStack = StackContents rest
        (top,newStack)
    | [] -> 
        failwith "Stack underflow"

// ==============================================
// Operator core
// ==============================================

// pop the top two elements
// do a binary operation on them
// push the result
let binary mathFn stack = 
    let y,stack' = pop stack    
    let x,stack'' = pop stack'  
    let z = mathFn x y
    push z stack''  

let isInt str = 
    let rx = System.Text.RegularExpressions.Regex @"^\d+$"
    rx.IsMatch str

let isOperator str =
    let rx = System.Text.RegularExpressions.Regex @"^[-+*/]$"
    rx.IsMatch str

// to be applied on stack
let apply str =
    match str with
    | "+" -> binary (+)
    | "-" -> binary (-)
    | "*" -> binary (*)
    | "/" -> binary (/)
    | _ -> failwith """Expects operator, like "+"."""

// creates a list of strings from the original one, white space characters serving as separators
let words = (fun (str:string) ->
    str.Split ' '
    |> Seq.toList)

// ==============================================
// Calculator
// ==============================================

// loops through list of stringed numbers and operators
// numbers are pushed to stack, when (binary) operator
// it is applied to 2 popped numbers and the result is
// pushed to the stack

// RPN ["8";"4";"3";"*";"+";"2";"/"] EMPTY -> Stack 10
// (3*4 + 8) / 2 = 10
let rec RPN stringl stack =
    match stringl with
    | head :: tail when (isInt head) -> RPN tail (push (float head) stack)
    | head :: tail when (isOperator head) -> RPN tail (apply head stack)
    | [] -> stack
    | _ -> failwith "RPN Error"
