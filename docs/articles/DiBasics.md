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
save operation on a data store. It is desirable to delegate these different
operations to separate classes, and include references to the corresponding
APIs that will be consumed. Expanding a bit in our example:
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
Writing the `SavePerson` this way, allows the specifics of the validation and storage of data to be independent from the Service. We then say that the `validator` and `repository` objects are *Injectable Dependecies*.

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