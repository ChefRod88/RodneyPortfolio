(function(){
  var canvas=document.getElementById('errCanvas'),ctx=canvas.getContext('2d'),W,H;
  function resize(){W=canvas.width=window.innerWidth;H=canvas.height=window.innerHeight}
  resize();window.addEventListener('resize',resize);
  var pts=Array.from({length:60},function(){return{x:Math.random()*2000,y:Math.random()*1000,vx:(Math.random()-.5)*.2,vy:(Math.random()-.5)*.2,r:Math.random()*1.2+.3,a:Math.random()*.28+.05}});
  (function loop(){ctx.clearRect(0,0,W,H);pts.forEach(function(p){p.x+=p.vx;p.y+=p.vy;if(p.x<0)p.x=W;if(p.x>W)p.x=0;if(p.y<0)p.y=H;if(p.y>H)p.y=0;ctx.beginPath();ctx.arc(p.x,p.y,p.r,0,Math.PI*2);ctx.fillStyle='rgba(201,168,76,'+p.a+')';ctx.fill()});requestAnimationFrame(loop)})();
})();
