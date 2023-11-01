using System;
using System.Collections.Generic;
using UnityEngine;

namespace Samples.ListView
{
    public class ContactListView : MonoBehaviour
    {
        [SerializeField] private ListView listView;
        [SerializeField] private List<Contact> contacts;
        [SerializeField] private float scrollSpeed = 10;

        private void Awake()
        {
            listView.AddDataBinder<Contact, ContactVisuals>(BindContact);

            listView.SetDataSource(contacts);
        }

        private void BindContact(Contact data, ContactVisuals visuals, int index)
        {
            visuals.Text.text = $"{data.userName} ({data.userID.ToString()})";
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                listView.Scroll(-scrollSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                listView.Scroll(scrollSpeed * Time.deltaTime);
            }
        }
    }
}