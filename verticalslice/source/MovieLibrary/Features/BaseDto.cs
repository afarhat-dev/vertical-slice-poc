using System;

public class BaseDto(Guid _Id)
{

    public Guid Id
    {
        get { return _Id; }
        set { _Id = value; }
    }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
