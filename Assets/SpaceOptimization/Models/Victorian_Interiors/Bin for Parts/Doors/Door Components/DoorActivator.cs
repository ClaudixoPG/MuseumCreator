

using UnityEngine;
using System.Collections;

public class DoorActivator : MonoBehaviour
{

        private Animator animator;
        public AudioClip openSound;
        public AudioClip closeSound;
        private AudioSource source;

    void Awake()
    {
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            animator.SetBool("Open", true);
            source.PlayOneShot(openSound, 1);

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            animator.SetBool("Open", false);
            source.PlayOneShot(closeSound, 1);
        }
    }
}
