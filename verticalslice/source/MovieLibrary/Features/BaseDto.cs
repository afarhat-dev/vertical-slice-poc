using System;

public class BaseDto(Guid _Id)
{

    public Guid Id
    {
        get { return _Id; }
        set { _Id = value; }
    }
}
