namespace Utils
{
    public record Either<a,b>;
    public record EitherA<a,b>(a Value) : Either<a, b>;
    public record EitherB<a, b>(b Value) : Either<a, b>;
}
