using UnityEngine;

public class DestroyAnimation : MonoBehaviour
{
    void Start()
    {
        Animator animation = this.GetComponent<Animator>();

        if (animation == null)
        {
            Destroy(gameObject, 2.5f);
        }
        else
        {
            Destroy(gameObject, animation.GetCurrentAnimatorStateInfo(0).length + 0.25f);
        }
    }
}
