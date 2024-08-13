<script setup>
    import '@/assets/rider/css/home.css' //导入样式
    import RiderHomeHeader from '@/components/rider/home/Header.vue' //导入 Header 组件
	import RiderHomeSide from '@/components/rider/home/Side.vue' //导入侧边栏组件
</script>

<template>
    <div class="dr-home">
        <RiderHomeHeader />
        <div class="main">
            <RiderHomeSide />
            <div class="content">
                <router-view />
            </div>
        </div>        
    </div>
</template>

<style scoped>

</style>
