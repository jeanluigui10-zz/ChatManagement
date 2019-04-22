﻿using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using xChatBusiness;
using xChatEntities;

namespace xChatAPI
{
    public class CounterHub : Hub
    {

        /// <summary>
        /// Se ejecuta cuando algun cliente se desconecta de forma no controlada, por ejemplo
        /// se cierra el navegador o la PC/Laptop entra en estado de hibernación.
        /// En este caso se debe desconectar de la DB.
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            ObjectResultList<ChatToken> tokenDestino = ServiceChatBL.Instancia.ChatDisconnected(Context.ConnectionId);

            foreach (ChatToken item in tokenDestino.Elements)
            {
                // Tipo 1: Usuario desconectado.  Notificar a Manager.
                // Tipo 2: Manager desconectado.  Notificar a Usuarios.
                if (item.TypeToken.Equals(1))
                    Clients.Client(item.Token).chatUserDisconnect();
                else
                    Clients.Client(item.Token).chatManagerDisconnect();
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnConnected()
        {
            // Clients = es un método que hereda de la clase Hub.
            // All = para todos los clientes.
            // enterUser() = es un método personalizado que se usa para los fines del chat.
            //               puede ser cualquier nombre, siempre en conntacion Cammell (minúscula, mayúscula).
            Clients.All.enterUser();

            string connectId = Context.ConnectionId;

            string name = Context.User.Identity.Name;

            return base.OnConnected();
        }

        /// <summary>
        /// Evento que es lanzado desde el User para enviar un mensaje al Manager.
        /// </summary>
        /// <param name="conversationEntity"></param>
        public void SendToManager(ConversationEntity conversationEntity)
        {
            conversationEntity.IsSendUser = 1;

            // ------------------------------------------------------------
            // Si es la primera vez que un cliente envía un mensaje,
            // entonces se genera el ID de la comunicación.
            // ------------------------------------------------------------
            if (conversationEntity.ChatId.Equals(0))
            {
                conversationEntity.UserToken = Context.ConnectionId;
                conversationEntity.ChatId = ServiceChatBL
                    .Instancia
                    .ChatCreate(conversationEntity);

                // ------------------------------------------------------------
                // Se lanza método en el front del Manager para agregar
                // un nuevo Chat en su monitor.
                // ------------------------------------------------------------
                Clients.Client(conversationEntity.ManagerToken).newUserConnect(conversationEntity);
            }

            // ------------------------------------------------------------
            // Registrar mensaje en la DB.
            // ------------------------------------------------------------
            ServiceChatBL.Instancia.ChatMessageCreate(conversationEntity);

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Manager.
            // ------------------------------------------------------------
            Clients.Client(conversationEntity.ManagerToken).receivedFromUser(conversationEntity);

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Usuario.
            // ------------------------------------------------------------
            Clients.Caller.receivedFromManager(conversationEntity);
        }

        /// <summary>
        /// Evento que es lanzado por el Manager para enviar un mensaje al User.
        /// </summary>
        /// <param name="conversationEntity"></param>
        public void SendToUser(ConversationEntity conversationEntity)
        {
            conversationEntity.IsSendUser = 0;

            // ------------------------------------------------------------
            // Registrar mensaje en la DB.
            // ------------------------------------------------------------
            ServiceChatBL.Instancia.ChatMessageCreate(conversationEntity);

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Usuario.
            // ------------------------------------------------------------
            Clients.Client(conversationEntity.UserToken).receivedFromManager(conversationEntity);

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Manager.
            // ------------------------------------------------------------
            Clients.Caller.receivedFromUser(conversationEntity);
        }

        /// <summary>
        /// Método que se ejecuta cuando se conecta un MANAGER.
        /// Se genera su Identificador (ID) de la comunicación.
        /// </summary>
        /// <param name="accountManagerEntity"></param>
        public void AccountManagerConnect(AccountManagerEntity accountManagerEntity)
        {
            accountManagerEntity.Token = Context.ConnectionId;
            ServiceChatBL.Instancia.AccountManagerConnect(accountManagerEntity);
            Clients.Caller.sucessConnect(Context.ConnectionId);
        }

        /// <summary>
        /// Método que se ejecuta cuando el MANAGER se desconecta.
        /// </summary>
        /// <param name="accountManagerEntity"></param>
        public void AccountManagerDisconnect(AccountManagerEntity accountManagerEntity)
        {
            accountManagerEntity.Token = Context.ConnectionId;
            ServiceChatBL.Instancia.AccountManagerDisconnect(accountManagerEntity);
            
        }

        /// <summary>
        /// Método que se ejecuta cuando el MANAGER desconecta a un usuario.
        /// </summary>
        /// <param name="conversationEntity"></param>
        public void UserDisconnectForMangaer(ConversationEntity conversationEntity)
        {
            conversationEntity.IsSendUser = 0;

            // ------------------------------------------------------------
            // Registrar mensaje en la DB.
            // ------------------------------------------------------------
            ServiceChatBL.Instancia.UserDisconnectForManager(conversationEntity);

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Usuario.
            // ------------------------------------------------------------
            Clients.Client(conversationEntity.UserToken).serverOrderDisconnect();

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Manager.
            // ------------------------------------------------------------
            Clients.Caller.receivedFromUserDisconnect(conversationEntity);
        }

        /// <summary>
        /// Establecer el CHATID como Leído por el MANAGER.
        /// Se notifica al Agente y Usuario que el mensaje ha sido Leído.
        /// </summary>
        /// <param name="conversationEntity"></param>
        public void SetMessageReadForManager(ConversationEntity conversationEntity)
        {
            conversationEntity.IsSendUser = 0;

            // ------------------------------------------------------------
            // Establecer chatID como Leído.
            // ------------------------------------------------------------
            ServiceChatBL.Instancia.SetMessageReadForManager(conversationEntity);

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Usuario.
            // ------------------------------------------------------------
            //Clients.Client(conversationEntity.UserToken).MessageRead();

            // ------------------------------------------------------------
            // Se lanza el método de los mensajes en el front del Manager.
            // ------------------------------------------------------------
            //Clients.Caller.MessageRead(conversationEntity);
        }
    }
}
