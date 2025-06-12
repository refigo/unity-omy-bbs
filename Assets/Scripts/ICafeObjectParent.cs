using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICafeObjectParent
{
    public Transform GetCafeObjectFollowTransform();

    public void SetCafeObject(CafeObject cafeObject);

    public CafeObject GetCafeObject();

    public void ClearCafeObject();

    public bool HasCafeObject();
}
