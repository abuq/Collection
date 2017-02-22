phantom.outputEncoding="gb2312";


var page = require("webpage").create();


page.settings.userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";

//page.settings.encoding = "gzip, deflate, sdch, br";

//page.settings.language = "zh-CN,zh;q=0.8";



//page.settings.referrer = "https://www.zhihu.com";





page.open("https://www.zhihu.com/#signin",function(status){

	console.log('Status:'+status);
	
	var title = page.evaluate(function(){
		
		
		var inputList = document.getElementsByTagName("input");
		
		for(var i=0;i<inputList.length;i++)
		{
			//&&inputList[i].type="text"
			if(inputList[i].name === "account")
				inputList[i].value = "15757135981";
			//&&inputList[i].type="password"
			else if(inputList[i].name === "password")
				inputList[i].value = "huxiaofei";
			else
				;
		}
		
		var buttonList = document.getElementsByTagName("button");
		
		for(var i=0;i<buttonList.length;i++)
		{
			if(buttonList[i].className==="sign-button submit")
			{
				buttonList[i].click();
				break;
			}
		}
		
		
		
		return document.title;
	});
	 

	window.setTimeout(function(){  
	console.log(title);
	
	page.render("zhihu.png");
	
	phantom.exit();
	},5000);
	
});