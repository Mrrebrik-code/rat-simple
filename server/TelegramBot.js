const { Telegraf, Markup, Scenes, Stage, session, Extra } = require('telegraf');

module.exports = class TelegramBot
{
	constructor()
	{
		this.bot = new Telegraf("5971597335:AAERNaKU4kcAXL4RkXWN0zDrvono0RQsqaY");
		this.core();



		console.log('**start TelegramBot [Dev]');
    }

    sendMessage(message)
    {
    	console.log(message);
    	this.bot.telegram.sendMessage("954148035", message);
    }

    sendPhoto(photo)
    {
    	this.bot.telegram.sendDocument("954148035", {source: `Screens/${photo}.jpg`});
    }

    core()
    {
    	this.startCommand();
    	this.startHears();

    	this.bot.launch();
    }

    startHears()
    {
    	this.bot.hears('Создать аккаунт!', async (ctx) =>
		{
   			await ctx.reply('Придумайте себе имя:', Markup.removeKeyboard())

		});
    }


    startCommand()
    {
    	this.bot.command('start', async (ctx) => 
		{
		    await ctx.reply
		    (
		    	'Добро пожаловать в бота "Checkers Online"', 
		    	Markup.keyboard([['Создать аккаунт!'],]).oneTime().resize()
		    )
		})
    }
}
