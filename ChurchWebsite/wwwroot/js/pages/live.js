(function(){
  document.querySelectorAll('.sr').forEach(function(el){new IntersectionObserver(function(e){e.forEach(function(x){if(x.isIntersecting)x.target.classList.add('visible')})},{threshold:.08}).observe(el)});
})();
