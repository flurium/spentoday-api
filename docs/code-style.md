# Code style

The document describes principles to keep code in good conditions.

## Errors

All internal functions (ones you write by your hands, not from library or framework) should
return exception as value. Or it should return just true/false. But any internal
`function must not throw error` without super specific reason.

Examples of return types to not throw an exception:

- true/false: true if success, false if not
- enums: if more than 2 indicators
- nullable T: null if error, T if success return
- { T1?, T2? }: if you have 2 or more return situation, that require inner parameters

## Nesting, if else

You should avoid nesting as much as possible without making code too hard to read.
Nesting if when you have inner block like this:

```csharp
public bool Check(int num) {

  if(num > 0) {
    return true
  } else {

    // nesting
    if(num < 10) {
      return true
    } else {
      return false
    }
  }
}
```

To avoid nesting you should do `early return`.
Code become more readable, look:

```csharp
public bool Check(int num) {
  if(num > 0) return true
  if(num < 10) return true

  return false
}
```

The amount of tabs was reduced significantly.
Less levels = more readable code.

## Required fields

You should use constructors for classes. All required fields should appear in constructor. It's the rule.

```csharp
public class User {
  // name isn't required
  public string Name { get; set; }

  // email is required
  public string Email { get; set; }

  public User(string email) {
    Email = email;
  }
}
```

## Data classes

If class is just container for data, you should use records or tuples. Records can be used for Dto, tuples can be used for return or input types.
You should place data classes as close as possible to methods that use it.

```csharp

// tuple as return type
public (string?, Exception?) UserId(string token) {
  try {
    var uid = SomeMagic(token);
    return (uid, null);
  } catch (Exception ex) {
    return (null, ex);
  }
}

// record is immutable
public class record ProductDto(string Name, string Description);

public ProductDto GetProducts() {
  var product = db.Products.QueryOne();
  return new ProductDto(product.Name, product.Description);
}

```
