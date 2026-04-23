using UnityEngine;

public class FakeBlock : MonoBehaviour
{
    [SerializeField]
    private Animation _animation = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            _animation.Play();
        }
    }
}
