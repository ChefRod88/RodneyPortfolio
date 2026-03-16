(function(){
  var obs=new IntersectionObserver(function(e){e.forEach(function(x,i){if(x.isIntersecting)setTimeout(function(){x.target.classList.add('visible')},i*80)})},{threshold:.08});
  document.querySelectorAll('.gr-card').forEach(function(el){obs.observe(el)});
})();
