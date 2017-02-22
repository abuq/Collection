phantom.outputEncoding="gb2312";


var page = require("webpage").create();

var system = require('system');

var url;

if(system.args.length==1)
{
	phantom.exit(1);
}else
{
	url = system.args[1];
	
	
	page.settings.userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";



	page.open(url,function(status){

		//console.log('Status:'+status);
	
		var title = page.evaluate(function(){
		
			return document.title;
		});
	 

		window.setTimeout(function(){  
	
	
		//console.log(title);
		//console.log(page.content);
		//console.log(url);
		var cookies = page.cookies;
		
		var length = cookies.length;
		
		//[object Object],[object Object],[object Object],[object Object],[object Object],[object Object],[object Object]
		//console.log(cookies);
		//7
		//console.log(length);
		
		var cookieString = '';
		
		//这里得到的是编号 有点奇怪
		for(var i in cookies)
		{
			if(i<length-1)
				cookieString += cookies[i].name+'='+cookies[i].value+';';
			else
				cookieString += cookies[i].name+'='+cookies[i].value;
		}
		
		console.log(cookieString);
	
		page.render("taobaoCookie.png");
	
		phantom.exit();
		},5000);
		
	
});

}

