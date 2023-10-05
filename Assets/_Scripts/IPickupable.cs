using Unity.Netcode;

public interface IPickupable<T>
{
    public /*T*/ void Pickup(NetworkObject returnTo);
}
