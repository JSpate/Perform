# ShowScript Language Documentation  

## Overview  
ShowScript is a domain-specific language designed for scripting events, device interactions, and parameter manipulations in a show or performance context. It supports variable declarations, event handling, conditional logic, loops, and function calls.  

---

## Grammar Rules  

### 1. Script Structure  
The root rule defines the structure of a script:  
- **`script`**: A script consists of constant declarations, variable declarations, event blocks, and statements.
---

### 2. Declarations  

#### 2.1 Constant Declaration  
Defines a constant value at the module level.  
- Syntax: `const <ID> = <expr>;`
#### 2.2 Variable Declaration  
Declares a variable with a specific type and assigns an initial value.  
- Syntax: `<typeName> <ID> = <expr>;`
#### 2.3 Supported Types  
The following types are supported:  
- `bool`, `int`, `float`, `string`, `long`, `double`, `Device`.
---

### 3. Event Handling  

#### 3.1 Event Block  
Defines an event handler with optional conditions and actions.  
- Syntax: `on(<eventType>(<args>)) when(<condition>) { <statements> }`
#### 3.2 Event Types  
Supported event types include:  
- `ButtonChange`, `BarChange`, `SectionChange`, `StartSong`, `EndSong`.
#### 3.3 Event Arguments  
Arguments can be expressions, enums, or identifiers. Named arguments are supported.
#### 3.4 ButtonState Enum  
Defines button states for event arguments.  
- Values: `ButtonState.Down`, `ButtonState.Up`, `ButtonState.Press`, `ButtonState.LongPress`.
---

### 4. Statements  

#### 4.1 General Statements  
Statements include function calls, assignments, variable declarations, control flow, and more.
#### 4.2 Set Block Statement  
Batch assignment of parameters to devices or groups.  
- Syntax: `set(<targets>) { <assignments> }`
---

### 5. Control Flow  

#### 5.1 If/Else Statement  
Conditional branching.  
- Syntax: `if (<condition>) { <statements> } else { <statements> }`
#### 5.2 For Loop  
Iterates over a range or condition.  
- Syntax: `for(<init>; <condition>; <increment>) { <statements> }`
#### 5.3 While Loop  
Repeats while a condition is true.  
- Syntax: `while (<condition>) { <statements> }`
#### 5.4 Try/Catch/Finally  
Handles exceptions.  
- Syntax:
---

### 6. Expressions  

#### 6.1 Ternary Expressions  
Conditional expressions.  
- Syntax: `<condition> ? <trueExpr> : <falseExpr>`
#### 6.2 Arithmetic and Logical Operators  
Supports standard operators:  
- Arithmetic: `+`, `-`, `*`, `/`, `%`.  
- Logical: `&&`, `||`, `!`.  
- Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`.  

---

### 7. Function Calls  

#### 7.1 Namespaces  
- **Song**: Functions like `Time()`, `Bar()`, `Beat()`.  
- **Motion**: Functions like `Circle()`, `Figure8()`.  
- **Color**: Functions like `RGB()`, `Blend()`.  
- **Math**: Functions like `Sin()`, `Pow()`.  
- **Lites**: Functions like `Set()`, `FadeIn()`.  

#### 7.2 Generic Calls  
Supports custom device or namespace calls.
---

### 8. Values and Identifiers  

#### 8.1 Values  
Supports literals:  
- Numbers (`int`, `float`), strings, booleans (`true`, `false`), `null`.
#### 8.2 Dotted Identifiers  
Used for hierarchical names (e.g., `Device.Group.Function`).
---

## Lexer Rules  

### Keywords  
Reserved keywords include:  
- `const`, `var`, `if`, `else`, `for`, `while`, `break`, `continue`, `return`, `try`, `catch`, `finally`.  

### Operators  
- Assignment: `=`  
- Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`.  
- Arithmetic: `+`, `-`, `*`, `/`, `%`.  
- Logical: `&&`, `||`, `!`.  

### Literals  
- Numbers: `123`, `3.14`.  
- Strings: `"Hello"`.  
- Booleans: `true`, `false`.  

---  

## Example Script
