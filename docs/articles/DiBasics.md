# Basics of Dependency Injection

In this article, we'll explore the basics of Dependency Injection patterns.

## What is Dependency Injection?
Generally speaking, for well designed applications, types should have pretty
simple and straightforward things to do. A class that has too many capabilities
can become unmaintainable and difficult to work with. With that in mind,
types can delegate implementation details onto other classes, and just consume
that type's API.

Let's see an example:

```csharp
public class PersonService
{
    public void SavePerson(Person person)
    {
        // ...
    }
}
```
The `SavePerson` method could include validation logic, as well as the acutal
save operation on a data store. However, it is desirable to delegate these
different operations to separate classes, and include references to the
corresponding APIs that will be consumed. Expanding a bit in our example:
```csharp
public class PersonService
{
    IValidator<Person> validator;
    IPersonRepository repository;

    public void SavePerson(Person person)
    {
        if (!validator.IsValid(person)) return;
        repository.Store(person);
    }
}
```
Writing the `SavePerson` this way, allows the specifics of the validation and
 storage of data to be independent from the Service. We then say that the
 `validator` and `repository` objects are *Injectable Dependecies*.

The way dependencies can be injected into an instance might be up to the
developer, but it's usually done with constructors.
```csharp
public class PersonService
{
    IValidator<Person> validator;
    IPersonRepository repository;

    public PersonService(IValidator<Person> validator, IPersonRepository repository)
    {
        this.validator = validator;
        this.repository = repository;
    }

    public void SavePerson(Person person)
    {
        if (!validator.IsValid(person)) return;
        repository.Store(person);
    }
}
```
To consume the `PersonService`, you must specify the constructor parameters, in
essence, *injecting* the required dependencies into your new `PersonService`
instance. This way, if there's ever a need to change behaviour or write unit
tests for your application, it's trivial to change the dependencies by just
using different implementations:
```csharp
PersonService svc;

// A person service that writes to an SQL databse and has a custom validator.
svc = new PersonService(new MyOwnPersonValidator(), new SqlPersonRepository());

// A person service that includes Mock-up dependencies for unit tests.
svc = new PersonService(new MockValidator<Person>(), new MockPersonRepository());
```
## The issues with Dependency Injection
As an application grows, you can end up with a lot of classes, each requiring a
set of dependencies that need to be specified for that class to function
correctly. At times, this might be overwhelming, like in the case of classes
that need several dependencies to be instanced.

There's also the issue of respecting the *dependency inversion* **SOLID**
principle. There's going to be a point where an assembly will just need to know
the specifics about the classes that need to be instanced and consumed, be it
during service registration or otherwise. This is allowed on the assembly in
charge of launching an application or service (the
*Entry Assembly*) but might be frowned upon if done in auxiliary libraries.

There's several ways to mitigate these problems. The first and most important
one is to keep *Liskov's substitution* and *single responsibility* principle in
mind, and use interfaces wherever feasible, with the intent of abstracting away
as much miscellaneous logic as possible in your bussiness logic.

As for dependency injection, by keeping your registrations in a single place at
the entry assembly you can avoid the traps of using a class somewhere with a
set of dependencies, and then wonder why you're trying to use that same class
the same way somewhere else just to have your application crash, unknowningly
to you, due to the use of a different set of dependencies.

This is where **`ServicePool`** can be used to simplify dependency injection.
It provides you with a repository of dependencies where all your services and
singletons, alongside their dependencies can live, and you can just ask for a
type of service (in the form of an interface) and **`ServicePool`** will
resolve its dependencies for you and return an instance that you can use.