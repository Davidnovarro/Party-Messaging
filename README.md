# Party-Messaging
Party Messaging is a powerful high-level message networking solution for Client / Server type of applications, made to simplify the networking communication. On a single decent Linux server, you can expect it to handle thousands of persistent connections and 100K+ messages per second.

### Features
- Persistent Alive connections.
- Message sending/listening.
- Queries: the request/response messaging style.
- Message batching: the ability to write multiple messages and send them as one packet to reduce overhead.
- Tolerance for the differences in message declarations: the field in a message declaration can be renamed, removed or added and it will continue to be operational.
- Nested values in messages.
- Prevent connection from message spamming.
- Support for custom low-level transport.
- Support for custom serialization.


## Network Client / Server example
There is no Client to Client connection so in this example we'll be sending Chat messages to the server and it will transmit them to receivers.

##### First declare the message type
Messages need to implement the IMessage interface and they can be both value or reference types.
The default built-in serialization is [Protobuf](https://github.com/protobuf-net/protobuf-net "Protobuf") it works is like JSON or XML but it is smaller and a lot faster.

By default, messages need to be decorated so the serializer will know which members to serialize and which ones to ignore.

```C#
[ProtoContract]
public struct ChatMessage : IMessage
{
        //Unique message ID it's used internally
        public ushort GetId { get { return id; } }

        //Generating and caching the unique number
        static readonly ushort id = MessageIdProvider.GetId<ChatMessage>();

        [ProtoMember(1)]
        public int senderId;

        [ProtoMember(2)]
        public int receiverId;

        [ProtoMember(3)]
        public string message;

        [ProtoMember(4)] //Nested value
        public ChatRoom chatRoom;
}

[ProtoContract]
public class ChatRoom
{
        public int roomId
}
```

##### Using the Client
```C#
private readonly NetworkClient Client = new TelepathyClient();

static void Main()
{
        //Add message listsners
        Client.AddListener<ChatMessage>(OnChatMessageReceived);
        Client.Connect("127.0.0.1", 8888);

        while (true)
        {
            Client.Update();
            Thread.Sleep(1);
        }
}

public void SendChatMessage()
{
        var msg = new ChatMessage()
        {
            senderId = 0,
            receiverId = 5,
            message = "Some random text"
        };
        Client.Send(msg);
}

public void SendMultipleMessages()
{
    var msg1 = new ChatMessage()
    {
        receiverId = 5,
        message = "Some random text"
    };

    var msg2 = new ChatMessage()
    {
        receiverId = 3,
        message = "Some random text 2"
    };

    var msg3 = new SomeRandomMessage()
    {
        someField = "Foo"
    };

    //Sending batched messages, they will be received as usual ones
    Client.BatchMessage(msg1)
          .BatchMessage(msg2)
          .BatchMessage(msg3)
          .Send();
}

protected override void OnChatMessageReceived(NetworkConnection conn, ChatMessage msg)
{
        Console.WriteLine("I've received a messssage from {1} : {2}", msg.senderId, msg.message);
}
```


##### Using the Server
The server will be listening for Chat messages and transmitting them to receivers.

```C#
private readonly NetworkServer Server = new TelepathyServer();

static void Main()
{
        //Add message listsners
        Server.AddListener<ChatMessage>(OnChatMessageReceived);

        //Bind the port and start the server
        Server.Start(8888);
        while (true)
        {
            Server.Update();
            Thread.Sleep(1);
        }
}

protected override void OnChatMessageReceived(NetworkConnection conn, ChatMessage msg)
{
        //Transmit the message to the receiver
        var receiver = Server.Connections[msg.receiverId];
        Server.Send(receiver, msg);
        Console.WriteLine("{0} jsut sent the messssage : {1} to the client {2}", msg.senderId, msg.message, msg.receiverId);
}
```

## Queries
Queries are a simple way to communicate in a request/response style. Usually, this method is used only when communicating from Client > to > Server, but Party-Messaging supports any type of connection, both Client and Server can listen for a query message and send a response for it.
For example, you may want the Server to ask the Client what is he doing currently and in case if there is no response (timeout) perform certain actions.

##### Declaring the query messages
Query messages need to implement IRequestMessage / IResponseMessage interface. The Structures are the same as for the IMessage except that they should have an unsigned short named QueryId which is used internally to identify and invoke the response callback.

```C#
[ProtoContract]
public struct GetUserStatusRequest : IRequestMessage
{
    public ushort GetId { get { return _mid; } }

    static readonly ushort _mid = MessageIdProvider.GetId<GetUserStatusRequest>();

    [ProtoMember(1)]      //This is used internally, has to be serialized
    public ushort QueryId { get; set; }

    [ProtoMember(2)]
    public int userId;
}

[ProtoContract]
public struct GetUserStatusResponse : IResponseMessage
{
    public ushort GetId { get { return _mid; } }

    static readonly ushort _mid = MessageIdProvider.GetId<GetUserStatusResponse>();

    [ProtoMember(1)]      //This is used internally, has to be serialized
    public ushort QueryId { get; set; }

    [ProtoMember(2)]
    public int userId;

    [ProtoMember(3)]
    public string userStatus;
}
```
##### Listening for requests and responding to them 
Queries module is responsible for everything related to requests and responses.
```C#
void Initialize()
{
    //Add listener for the Requests
    Server.Queries.AddListener<GetUserStatusRequest>(OnStatusRequest);
    Server.Start(8888);
}

private void OnStatusRequest(NetworkConnection conn, GetUserStatusRequest request)
{
    var response = new GetUserStatusResponse
    {
        userId = request.userId,
    };

    if (IsUserOnline(request.userId))
        response.userStatus = "ONLINE";
    else
        response.userStatus = "OFFLINE";

    Server.Queries.Respond(conn, response, request);
}
```

##### Sending the Request
```C#
public void SendRequest()
{
    var request = new GetUserStatusRequest()
    {
        userId = 1008
    };

    Client.Queries.Request<GetUserStatusRequest, GetUserStatusResponse>(serverConnection, request,
        //Set response callback
        (conn, response) =>
        {
            Console.WriteLine("Received user status {0}", response.userStatus);
        });
}

public void SendRequestWithErrorCallback()
{
    var request = new GetUserStatusRequest()
    {
        userId = 1008
    };

    Client.Queries.Request<GetUserStatusRequest, GetUserStatusResponse>(serverConnection, request,
        //Set response callback
        (conn, response) =>
        {
            Console.WriteLine("Received user status {0}", response.userStatus);
        },
        //Set error callback
        (conn, error) =>
        {
            Console.WriteLine("Error message received {0}", error.ToString());
        }, 3.5); //Set time out for 3.5 seconds
}
```
