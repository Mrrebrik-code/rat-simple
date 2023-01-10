const { Server } = require("socket.io");
const readFileSync = require('fs');
const io = new Server({ /* options */ });
const telegramBot = require("./TelegramBot.js");
const bot = new telegramBot();

io.on("connection", (socket) => 
{
    console.log("connection");

    socket.on("screenshoot", (data, name) =>
    {
        console.log(data);
        saveImage(name, data);
    })

    socket.emit("screenshoot-sending", true);
});

io.listen(3000);

function saveImage(filename, data)
{
  var myBuffer = new Buffer(data.length);

  for (var i = 0; i < data.length; i++) 
  {
      myBuffer[i] = data[i];
  }

  readFileSync.writeFile('Screens/' + filename + ".jpg", myBuffer, function(err) 
  {
      if(err) 
      {
          console.log(err);
      } 
      else 
      {
          console.log("The file was saved!");
          bot.sendPhoto(filename);
      }
  });
}
