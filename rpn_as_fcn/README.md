# Polish Calculator
First project using F#

loops through list of stringed numbers and operators
numbers are pushed to stack, when (binary) operator
it is applied to 2 popped numbers and the result is
pushed to the stack



Modifying
https://fsharpforfunandprofit.com/posts/stack-based-calculator/

and made to run as Azure Function thanks to
https://www.aaron-powell.com/posts/2020-01-13-creating-azure-functions-in-fsharp/ 



Usage, to calculate  (3*4 + 8) / 2 = 10:

$ RPN ["8";"4";"3";"*";"+";"2";"/"] EMPTY;;

$ val it : Stack = StackContents [10.0]
