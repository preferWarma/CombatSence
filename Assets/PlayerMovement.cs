using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpSpeed;
        [Range(0, .3f)]
        [SerializeField] private float checkRadius = .2f;
        new private Rigidbody2D rigidbody;
        private Animator animator;

        private float inputX;
        [SerializeField] private bool isGround = true;
        [SerializeField] private LayerMask layer;

        void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            inputX = Input.GetAxisRaw("Horizontal");
            isGround = Physics2D.OverlapCircle(transform.position, checkRadius, layer);

            Move();
            Jump();
            Flip();
        }

        private void Flip()
        {
            if (inputX == -1)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (inputX == 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }

        private void Jump()
        {
            if (isGround && Input.GetButtonDown("Jump"))
            {
                rigidbody.velocity = new Vector2(0, jumpSpeed);
                animator.SetTrigger("Jump");
            }
        }

        private void Move()
        {
            rigidbody.velocity = new Vector2(inputX * moveSpeed, rigidbody.velocity.y);

            animator.SetBool("isGround", isGround);
            animator.SetFloat("Horizontal", rigidbody.velocity.x);
            animator.SetFloat("Vertical", rigidbody.velocity.y);
        }
    }
}